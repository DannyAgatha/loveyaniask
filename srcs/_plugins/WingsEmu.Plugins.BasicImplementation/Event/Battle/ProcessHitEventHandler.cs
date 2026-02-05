// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Packets.Enums.Rainbow;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;

namespace NosEmu.Plugins.BasicImplementations.Event.Battle;

public class ProcessHitEventHandler : IAsyncEventProcessor<ProcessHitEvent>
{
    private static readonly byte MAX_TARGETS = 50;
    private readonly IBattleEntityDumpFactory _battleEntityDumpFactory;
    private readonly IBCardEffectHandlerContainer _bCardEffectHandler;
    private readonly IDamageAlgorithm _damageAlgorithm;
    private readonly IRandomGenerator _randomGenerator;
    private readonly ISkillUsageManager _skillUsageManager;
    private readonly RainbowBattleConfiguration _rainbowBattleConfiguration;

    public ProcessHitEventHandler(IBattleEntityDumpFactory battleEntityDumpFactory, IDamageAlgorithm damageAlgorithm,
        ISkillUsageManager skillUsageManager, IBCardEffectHandlerContainer bCardEffectHandler, IRandomGenerator randomGenerator, RainbowBattleConfiguration rainbowBattleConfiguration)
    {
        _battleEntityDumpFactory = battleEntityDumpFactory;
        _damageAlgorithm = damageAlgorithm;
        _skillUsageManager = skillUsageManager;
        _bCardEffectHandler = bCardEffectHandler;
        _randomGenerator = randomGenerator;
        _rainbowBattleConfiguration = rainbowBattleConfiguration;
    }

    public async Task HandleAsync(ProcessHitEvent e, CancellationToken cancellation)
    {
        IBattleEntity caster = e.Caster;

        if (!caster.IsAlive())
        {
            caster.CancelCastingSkill();
            return;
        }

        IBattleEntity target = e.Target;
        SkillInfo skill = e.HitInformation.Skill;
        Position position = e.HitInformation.Position;
        
        BCardDTO[] afterAttackAllAllies = skill.BCardsType.TryGetValue(SkillCastType.AFTER_ATTACK_ALL_ALLIES, out HashSet<BCardDTO> allAllies) ? allAllies.ToArray() : Array.Empty<BCardDTO>();
        BCardDTO[] beforeAttackOnMainTarget = skill.BCardsType.TryGetValue(SkillCastType.BEFORE_ATTACK_ON_MAIN_TARGET, out HashSet<BCardDTO> beforeAttackMain)
            ? beforeAttackMain.ToArray()
            : Array.Empty<BCardDTO>();
        BCardDTO[] beforeAttackSelf = skill.BCardsType.TryGetValue(SkillCastType.BEFORE_ATTACK_SELF, out HashSet<BCardDTO> beforeAttack) ? beforeAttack.ToArray() : Array.Empty<BCardDTO>();
        BCardDTO[] beforeAttackAllTargets = skill.BCardsType.TryGetValue(SkillCastType.BEFORE_ATTACK_ALL_TARGETS, out HashSet<BCardDTO> beforeAttackAll)
            ? beforeAttackAll.ToArray()
            : Array.Empty<BCardDTO>();

        IBattleEntityDump attacker = caster switch
        {
            IPlayerEntity character => _battleEntityDumpFactory.Dump(character, skill),
            IMonsterEntity monster => _battleEntityDumpFactory.Dump(monster, skill),
            INpcEntity mapNpc => _battleEntityDumpFactory.Dump(mapNpc, skill),
            IMateEntity mate => _battleEntityDumpFactory.Dump(mate, skill),
            _ => null
        };

        if (attacker == null)
        {
            caster.CancelCastingSkill();
            return;
        }

        List<IBattleEntity> entities;

        TargetHitType hitType = skill.HitType;

        (int randomChance, int range) =
            caster.BCardComponent.GetAllBCardsInformation(BCardType.SpecialDamageAndExplosions, (byte)AdditionalTypes.SpecialDamageAndExplosions.SurroundingDamage, caster.Level);

        if (randomChance != 0 && _randomGenerator.RandomNumber() <= randomChance && hitType is not TargetHitType.SpecialArea)
        {
            hitType = TargetHitType.EnemiesInAffectedAoE;
            skill.AoERange += (byte)range;
        }

        if (skill.TargetType == TargetType.NonTarget)
        {
            entities = position.GetEnemiesInRange(caster, (short)(skill.AoERange + 3)).ToList();
            if (skill.BCards.Any(x => (BCardType)x.Type == BCardType.FalconSkill && x.SubType == (byte)AdditionalTypes.FalconSkill.FalconFocusLowestHP))
            {
                if (entities.Count != 0)
                {
                    entities = Lists.Create(entities.OrderBy(x => x.HpPercentage).First());
                }
            }
        }
        else
        {
            switch (hitType)
            {
                case TargetHitType.EnemiesInAffectedAoE:
                    entities = skill.Vnum == (short)SkillsVnums.DOUBLE_RIPPER ? position.GetEnemiesInRange(caster, skill.AoERange).ToList() : target.GetEnemiesInRange(caster, skill.AoERange).ToList();

                    break;
                case TargetHitType.SpecialArea:
                    entities = _skillUsageManager.GetMultiTargets(caster.Id)
                        .Select(x => caster.MapInstance.GetBattleEntity(x.Item1, x.Item2))
                        .Where(x => x != null && caster.Position.IsInAoeZone(x.Position, 10) && caster.IsEnemyWith(x))
                        .ToList();
                    _skillUsageManager.ResetMultiTargets(caster.Id);
                    break;
                case TargetHitType.TargetOnly:
                    entities = Lists.Create(target);
                    break;
                default:
                    caster.CancelCastingSkill();
                    return;
            }

            if (!caster.Equals(target))
            {
                entities.SetFirst(target);
            }
        }

        if (!caster.IsPlayer() || caster is IPlayerEntity playerEntity && playerEntity.CheatComponent.HasNoTargetLimit == false)
        {
            entities = entities.Take(MAX_TARGETS).ToList();
        }

        foreach (BCardDTO bCard in beforeAttackOnMainTarget)
        {
            _bCardEffectHandler.Execute(target, caster, bCard, skill, position);
        }

        foreach (BCardDTO bCard in beforeAttackSelf)
        {
            _bCardEffectHandler.Execute(caster, caster, bCard, skill, position);
        }

        var targets = new List<(IBattleEntity, DamageAlgorithmResult)>();
        foreach (IBattleEntity entity in entities)
        {
            if (caster is IPlayerEntity c)
            {
                if (c.CheatComponent.IsInvisible)
                {
                    continue;
                }

                if (entity.IsMonster())
                {
                    var monster = entity as IMonsterEntity;
                    sbyte monsterMinRange = monster.MinimumAttackRange;
                    if (monsterMinRange != 0 && skill.Range < monsterMinRange)
                    {
                        continue;
                    }
                }
            }

            foreach (BCardDTO bCard in beforeAttackAllTargets)
            {
                _bCardEffectHandler.Execute(entity, caster, bCard, skill, position);
            }

            IBattleEntityDump defender = entity switch
            {
                IPlayerEntity character => _battleEntityDumpFactory.Dump(character, skill, true, entity.IsSameEntity(target)),
                IMonsterEntity monster => _battleEntityDumpFactory.Dump(monster, skill, true, entity.IsSameEntity(target)),
                INpcEntity mapNpc => _battleEntityDumpFactory.Dump(mapNpc, skill, true, entity.IsSameEntity(target)),
                IMateEntity mate => _battleEntityDumpFactory.Dump(mate, skill, true, entity.IsSameEntity(target)),
                _ => null
            };

            if (defender == null)
            {
                caster.CancelCastingSkill();
                continue;
            }

            DamageAlgorithmResult algorithmResult = _damageAlgorithm.GenerateDamage(attacker, defender, skill);
            targets.Add((entity, algorithmResult));

            if (caster.IsEnemyWith(entity))
            {
                continue;
            }

            foreach (BCardDTO bCard in afterAttackAllAllies)
            {
                _bCardEffectHandler.Execute(entity, caster, bCard, skill, position);
            }
        }

        foreach (BCardDTO bCard in afterAttackAllAllies)
        {
            _bCardEffectHandler.Execute(caster, caster, bCard, skill, position);
        }

        caster?.MapInstance.AddHitRequest(new HitRequest(targets, e.HitInformation, target));
    }
}