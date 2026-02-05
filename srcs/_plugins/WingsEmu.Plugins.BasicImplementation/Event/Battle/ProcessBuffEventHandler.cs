using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Mates;
using WingsEmu.Packets.Enums.Battle;

namespace NosEmu.Plugins.BasicImplementations.Event.Battle;

public class ProcessBuffEventHandler : IAsyncEventProcessor<ProcessBuffEvent>
{
    private static readonly byte MAX_TARGETS = 50;

    private readonly IBCardEffectHandlerContainer _bCardEffectHandler;
    private readonly RainbowBattleConfiguration _rainbowBattleConfiguration;

    public ProcessBuffEventHandler(IBCardEffectHandlerContainer bCardEffectHandler, RainbowBattleConfiguration rainbowBattleConfiguration)
    {
        _bCardEffectHandler = bCardEffectHandler;
        _rainbowBattleConfiguration = rainbowBattleConfiguration;
    }

    public async Task HandleAsync(ProcessBuffEvent e, CancellationToken cancellation)
    {
        IBattleEntity caster = e.Caster;
        IBattleEntity target = e.Target;
        SkillCast skillCast = e.SkillCast;
        SkillInfo skill = skillCast.Skill;
        Position position = e.Position;

        BCardDTO[] beforeAttackOnMainTarget = skill.BCardsType.TryGetValue(SkillCastType.BEFORE_ATTACK_ON_MAIN_TARGET, out HashSet<BCardDTO> beforeOnMain)
            ? beforeOnMain.ToArray()
            : Array.Empty<BCardDTO>();
        BCardDTO[] beforeAttackSelf = skill.BCardsType.TryGetValue(SkillCastType.BEFORE_ATTACK_SELF, out HashSet<BCardDTO> beforeSelf) ? beforeSelf.ToArray() : Array.Empty<BCardDTO>();
        BCardDTO[] beforeAttackAllTargets = skill.BCardsType.TryGetValue(SkillCastType.BEFORE_ATTACK_ALL_TARGETS, out HashSet<BCardDTO> beforeAttackAll)
            ? beforeAttackAll.ToArray()
            : Array.Empty<BCardDTO>();

        IEnumerable<IBattleEntity> entities = null;

        if (skill.TargetType == TargetType.NonTarget)
        {
            switch (skill.TargetAffectedEntities)
            {
                case TargetAffectedEntities.DebuffForEnemies:
                    entities = position.GetEnemiesInRange(caster, skill.AoERange);
                    break;
                case TargetAffectedEntities.BuffForAllies:
                    entities = skill.HitType switch
                    {
                        TargetHitType.TargetOnly => Lists.Create(caster),
                        TargetHitType.AlliesInAffectedAoE => position.GetAlliesInRange(caster, skill.AoERange)
                    };
                    break;
            }
        }
        else
        {
            switch (skill.TargetAffectedEntities)
            {
                case TargetAffectedEntities.DebuffForEnemies:
                    entities = skill.HitType switch
                    {
                        TargetHitType.TargetOnly => Lists.Create(target),
                        TargetHitType.EnemiesInAffectedAoE => target.GetEnemiesInRange(caster, skill.AoERange),
                        TargetHitType.PlayerAndHisMates => target.GetEnemiesInRange(caster, skill.AoERange).Where(x => x is IMateEntity mate && mate.Owner?.Id == caster.Id && mate.IsTeamMember),
                        _ => null
                    };
                    break;
                case TargetAffectedEntities.BuffForAllies:
                    entities = skill.HitType switch
                    {
                        TargetHitType.TargetOnly => Lists.Create(target),
                        TargetHitType.AlliesInAffectedAoE => target.GetAlliesInRange(caster, skill.AoERange),
                        TargetHitType.PlayerAndHisMates => target.GetAlliesInRange(caster, skill.AoERange).Where(x => x is IMateEntity mate && mate.Owner?.Id == caster.Id && mate.IsTeamMember),
                        _ => null
                    };
                    break;
            }
        }

        if (entities == null)
        {
            caster.CancelCastingSkill();
            return;
        }

        var entitiesToReturn = entities.Take(MAX_TARGETS).ToList();
        if (skill.TargetType == TargetType.Self && skill.TargetAffectedEntities == TargetAffectedEntities.BuffForAllies && !entitiesToReturn.Contains(caster))
        {
            entitiesToReturn.Add(caster);
        }

        foreach (BCardDTO bCard in beforeAttackSelf)
        {
            _bCardEffectHandler.Execute(caster, caster, bCard, skill, position);
        }

        if (target != null && skill.TargetType != TargetType.NonTarget)
        {
            foreach (BCardDTO bCard in beforeAttackOnMainTarget)
            {
                _bCardEffectHandler.Execute(target, caster, bCard, skill, position);
            }
        }

        foreach (BCardDTO bCard in beforeAttackAllTargets)
        {
            foreach (IBattleEntity entity in entitiesToReturn)
            {
                _bCardEffectHandler.Execute(entity, caster, bCard, skill, position);
            }
        }

        caster.MapInstance.AddBuffRequest(new BuffRequest(caster, entitiesToReturn, skillCast, position, target));
    }
}