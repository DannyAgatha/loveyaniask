using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Quicklist;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Maps;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;
using WingsEmu.Packets.ServerPackets.Battle;
using static WingsEmu.Packets.Enums.AdditionalTypes;
using Buff = WingsEmu.Game.Buffs.Buff;

namespace NosEmu.Plugins.BasicImplementations.Event.Battle;

public class ApplyProcessedHitEventHandler : IAsyncEventProcessor<ApplyHitEvent>
{
    private readonly IBCardEffectHandlerContainer _bCardHandlerContainer;
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IMonsterEntityFactory _monsterEntityFactory;
    private readonly IRandomGenerator _randomGenerator;
    private readonly ISkillUsageManager _skillUsageManager;
    private readonly ISacrificeManager _sacrificeManager;
    private readonly IBuffFactory _buffFactory;
    private readonly RainbowBattleConfiguration _rainbowBattleConfiguration;

    public ApplyProcessedHitEventHandler(IAsyncEventPipeline eventPipeline,
        IBCardEffectHandlerContainer bCardHandlerContainer, ISkillUsageManager skillUsageManager, IRandomGenerator randomGenerator,
        IMonsterEntityFactory monsterEntityFactory, ISacrificeManager sacrificeManager, IBuffFactory buffFactory, RainbowBattleConfiguration rainbowBattleConfiguration)
    {
        _eventPipeline = eventPipeline;
        _bCardHandlerContainer = bCardHandlerContainer;
        _skillUsageManager = skillUsageManager;
        _randomGenerator = randomGenerator;
        _monsterEntityFactory = monsterEntityFactory;
        _sacrificeManager = sacrificeManager;
        _buffFactory = buffFactory;
        _rainbowBattleConfiguration = rainbowBattleConfiguration;
    }

    public async Task HandleAsync(ApplyHitEvent e, CancellationToken cancellation)
    {
        HitInformation hit = e.HitInformation;

        IBattleEntity caster = hit.Caster;
        IBattleEntity target = e.Target;
        
        DamageAlgorithmResult algorithmResult = e.ProcessResults;
        HitType hitType = algorithmResult.HitType;
        int totalDamage = algorithmResult.Damages;

        SkillInfo skill = hit.Skill;

        BCardDTO[] afterAttackAllTargets = skill.BCardsType.TryGetValue(SkillCastType.AFTER_ATTACK_ALL_TARGETS, out HashSet<BCardDTO> bCards) ? bCards.ToArray() : Array.Empty<BCardDTO>();

        if (hit.IsFirst)
        {
            switch (skill.TargetType)
            {
                case TargetType.Self when skill.HitType == TargetHitType.EnemiesInAffectedAoE:
                    caster.BroadcastSuPacket(caster, skill, 0, SuPacketHitMode.NoDamageFail, isFirst: hit.IsFirst);
                    break;
                case TargetType.NonTarget:
                    caster.BroadcastNonTargetSkill(e.HitInformation.Position, skill);
                    break;
            }
        }

        if (target.BuffComponent.HasBuff((int)BuffVnums.SIDESTEP))
        {
            int secondData = target.BCardComponent.GetAllBCardsInformation(BCardType.MysticArts,
                (byte)AdditionalTypes.MysticArts.DodgeAndMakeChance, target.Level).secondData;

            target.RemoveBuffAsync((int)BuffVnums.SIDESTEP).ConfigureAwait(false).GetAwaiter().GetResult();
            target.AddBuffAsync(_buffFactory.CreateBuff(secondData, target)).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        if (target.BCardComponent.HasBCard(BCardType.Damage, (byte)Damage.HPIncreasedEveryAttackReceived))
        {
            (int firstData, int secondData) = target.BCardComponent.GetAllBCardsInformation(BCardType.Damage,
                (byte)Damage.HPIncreasedEveryAttackReceived, target.Level);
            
            target.HitsReceived++;

            if (target.HitsReceived <= secondData && !target.BCardComponent.HasBCard(BCardType.FourthGlacernonFamilyRaid, (byte)FourthGlacernonFamilyRaid.DisableHPMPRecovery))
            {
                int healAmount = target.MaxHp * firstData / 100; 
                target.Hp = Math.Min(target.Hp + healAmount, target.MaxHp);
                            
                target.BroadcastHeal(healAmount);
                
                await target.EmitEventAsync(new BattleEntityHealEvent
                {
                    Entity = target,
                    HpHeal = healAmount
                });
            }
        }
        
        if (target.BCardComponent.HasBCard(BCardType.Damage, (byte)Damage.MPIncreasedEveryAttackReceived))
        {
            (int firstData, int secondData) = target.BCardComponent.GetAllBCardsInformation(BCardType.Damage,
                (byte)Damage.MPIncreasedEveryAttackReceived, target.Level);
            
            target.HitsReceived++;

            if (target.HitsReceived <= secondData && !target.BCardComponent.HasBCard(BCardType.FourthGlacernonFamilyRaid, (byte)FourthGlacernonFamilyRaid.DisableHPMPRecovery))
            {
                int mpRegen = target.MaxMp * firstData / 100;
                target.Mp = Math.Min(target.Mp + mpRegen, target.MaxMp);
                
                await target.EmitEventAsync(new BattleEntityHealEvent
                {
                    Entity = target,
                    MpHeal = mpRegen
                });
            }
        }

        if (target.BCardComponent.HasBCard(BCardType.FourthGlacernonFamilyRaid, (byte)AdditionalTypes.FourthGlacernonFamilyRaid.RemoveRandomDebuff))
        {
            int firstData = target.BCardComponent.GetAllBCardsInformation(BCardType.FourthGlacernonFamilyRaid,
                (byte)AdditionalTypes.FourthGlacernonFamilyRaid.RemoveRandomDebuff, target.Level).firstData;
            int secondData = target.BCardComponent.GetAllBCardsInformation(BCardType.FourthGlacernonFamilyRaid,
                (byte)AdditionalTypes.FourthGlacernonFamilyRaid.RemoveRandomDebuff, target.Level).secondData;

            if (_randomGenerator.RandomNumber() > secondData)
            {
                return;
            }

            await target.RemoveRandomNegativeBuff(firstData);
        }
        
        bool dragon = false;
        BuffVnums? dragonBuff = null;
        int additionalDragonDamage = 0;
        
        var dragonTypes = new Dictionary<(BCardType Type, byte SubType), (BuffVnums Buff, byte AdditionalType)>
        {
            [(BCardType.LordCalvinas, (byte)LordCalvinas.SpawnFireDragon)] = (BuffVnums.FIERY_BREATH, (byte)LordCalvinas.SpawnFireDragon),
            [(BCardType.LordCalvinas, (byte)LordCalvinas.SpawnIceDragon)] = (BuffVnums.ICY_BREATH, (byte)LordCalvinas.SpawnIceDragon),
            [(BCardType.LordCalvinas, (byte)LordCalvinas.SpawnMoonDragon)] = (BuffVnums.MOON_SHADOW, (byte)LordCalvinas.SpawnMoonDragon),
            [(BCardType.SESpecialist, (byte)SESpecialist.SpawnSkyDragon)] = (BuffVnums.HEAVENLY_LIGHT, (byte)SESpecialist.SpawnSkyDragon)
        };
        
        if (caster.BCardComponent.HasBCard(BCardType.LordCalvinas, (byte)LordCalvinas.SpawnNeutralDragon))
        {
            (_, additionalDragonDamage) = caster.BCardComponent.GetAllBCardsInformation(
                BCardType.LordCalvinas,
                (byte)LordCalvinas.SpawnNeutralDragon,
                caster.Level
            );
            dragon = e.ProcessResults.SpecialistDragonEffect;
        }
        else
        {
            KeyValuePair<(BCardType Type, byte SubType), (BuffVnums Buff, byte AdditionalType)> activeDragon = dragonTypes.FirstOrDefault(type => caster.BCardComponent.HasBCard(type.Key.Type, type.Key.SubType));

            if (activeDragon.Key != default)
            {
                dragonBuff = activeDragon.Value.Buff;
                (_, additionalDragonDamage) = caster.BCardComponent.GetAllBCardsInformation(
                    activeDragon.Key.Type,
                    activeDragon.Value.AdditionalType,
                    caster.Level
                );
                dragon = e.ProcessResults.SpecialistDragonEffect;
            }
        }

        if (totalDamage <= 0 && caster is not IMonsterEntity { IsMateTrainer: true, IsSparringMonster: true })
        {
            totalDamage = 1;
        }

        if (caster.IsPlayer())
        {
            var player = (IPlayerEntity)caster;
            player.SkillComponent.IsSkillInterrupted = false;
            player.SkillComponent.CanBeInterrupted = false;
        }

        SuPacketHitMode hitMode = GetHitMode(skill, hitType, hit.IsFirst);
        
        if (dragon && skill.CastId != 0)
        {
            int dragonDamage = (int)(additionalDragonDamage * 0.01 * totalDamage);
            await _eventPipeline.ProcessEventAsync(new EntityDamageEvent
            {
                Damaged = target,
                Damager = caster,
                Damage = dragonDamage,
                CanKill = false,
                SkillInfo = skill,
            }, cancellation);

            caster.MapInstance.Broadcast(caster.GenerateSuPacket(target, dragonDamage, hitMode, EffectType.DragonBuffEffect, true));

            if (dragonBuff.HasValue)
            {
                Buff buff = _buffFactory.CreateBuff((int)dragonBuff.Value, caster);
                await caster.AddBuffAsync(buff);
            }
        }

        if (!caster.IsAlive())
        {
            hitMode = SuPacketHitMode.OutOfRange;
            caster.BroadcastSuPacket(caster, skill, 0, hitMode, isFirst: hit.IsFirst);
            Skip(caster, skill);
            return;
        }

        switch (target)
        {
            case IMonsterEntity monsterEntity:
                monsterEntity.MapInstance.MonsterRefreshTarget(monsterEntity, caster, DateTime.UtcNow, true);
                break;
            case INpcEntity npcEntity:
                npcEntity.MapInstance.NpcRefreshTarget(npcEntity, caster);
                break;
        }

        int monsterSize = target switch
        {
            IMonsterEntity monsterEntity => monsterEntity.CellSize,
            INpcEntity npcEntity => npcEntity.CellSize,
            IMateEntity mateEntity => mateEntity.CellSize,
            _ => 0
        };

        int cellSizeBonus = target switch
        {
            IPlayerEntity => 7,
            _ => 3
        };

        if (skill.Vnum != -1 && skill.CastId != -1 &&
            skill.HitType == TargetHitType.TargetOnly && !caster.Position.IsInRange(target.Position, skill.Range + monsterSize + cellSizeBonus) && skill.AttackType != AttackType.Dash &&
            !skill.BCards.Any(x => x.Type == (short)BCardType.JumpBackPush && x.SubType == (byte)AdditionalTypes.JumpBackPush.JumpBackChance))
        {
            hitMode = SuPacketHitMode.OutOfRange;
            caster.BroadcastSuPacket(target, skill, 0, hitMode, isFirst: hit.IsFirst);
            Skip(caster, skill);
            return;
        }

        if (target.BCardComponent.HasBCard(BCardType.AbsorptionAndPowerSkill, (byte)AdditionalTypes.AbsorptionAndPowerSkill.AddDamageToHP))
        {
            (int firstData, int secondData) bCard =
                target.BCardComponent.GetAllBCardsInformation(BCardType.AbsorptionAndPowerSkill, (byte)AdditionalTypes.AbsorptionAndPowerSkill.AddDamageToHP, target.Level);
            if (_randomGenerator.RandomNumber() <= bCard.firstData)
            {
                double toHealPercentage = bCard.secondData * 0.01;
                int toHeal = (int)(totalDamage * toHealPercentage);
                await target.EmitEventAsync(new BattleEntityHealEvent
                {
                    Entity = target,
                    HpHeal = toHeal
                });

                caster.BroadcastSuPacket(target, skill, 0, GetHitMode(skill, HitType.Miss, hit.IsFirst), isFirst: hit.IsFirst);
                Skip(caster, skill);
                return;
            }
        }

        if (caster.BCardComponent.HasBCard(BCardType.AngerSkill, (byte)AdditionalTypes.AngerSkill.ReduceEnemyHPByDamageChance) && caster.Level <= target.Level)
        {
            (int firstData, int secondData) bCard =
                target.BCardComponent.GetAllBCardsInformation(BCardType.AngerSkill, (byte)AdditionalTypes.AngerSkill.ReduceEnemyHPByDamageChance, caster.Level);

            int damageFromPercent = totalDamage * bCard.firstData / 100;
            if (damageFromPercent >= 1 && _randomGenerator.RandomNumber() <= bCard.secondData)
            {
                if (caster.Hp - bCard.secondData <= 0)
                {
                    if (caster.Hp != 1)
                    {
                        caster.BroadcastDamage(caster.Hp - 1);
                    }

                    caster.Hp = 1;
                }
                else
                {
                    caster.BroadcastDamage(bCard.secondData);
                    caster.Hp -= bCard.secondData;
                }
            }
        }

        if (target.BCardComponent.HasBCard(BCardType.RockyHelmetArmour, (byte)AdditionalTypes.RockyHelmetArmour.DecreaseEnemyHP))
        {
            int bCard = target.BCardComponent.GetAllBCardsInformation(BCardType.RockyHelmetArmour, (byte)AdditionalTypes.RockyHelmetArmour.DecreaseEnemyHP, target.Level).firstData;

            if (caster.Hp - bCard <= 0)
            {
                if (caster.Hp != 1)
                {
                    caster.BroadcastDamage(caster.Hp - 1);
                }

                caster.Hp = 1;
            }
            else
            {
                caster.BroadcastDamage(bCard);
                caster.Hp -= bCard;
            }
        }

        if (target.BCardComponent.HasBCard(BCardType.RockyHelmetArmour, (byte)AdditionalTypes.RockyHelmetArmour.DecreaseEnemyMP))
        {
            if (target is IPlayerEntity playerEntity && caster.IsPlayer())
            {
                if (playerEntity.GetMaxArmorShellValue(ShellEffectType.ProtectMPInPVP) > 0)
                {
                    return;
                }
            }

            int bCard = target.BCardComponent.GetAllBCardsInformation(BCardType.RockyHelmetArmour, (byte)AdditionalTypes.RockyHelmetArmour.DecreaseEnemyMP, target.Level).firstData;

            if (caster.Mp - bCard <= 0)
            {
                caster.Mp = 1;
            }
            else
            {
                caster.Mp -= bCard;
            }
        }
        
        if (caster.IsPlayer())
        {
            var character = (IPlayerEntity)caster;

            await character.RemoveInvisibility();
            
            if (character.BuffComponent.HasBuff((short)BuffVnums.SWARM_OF_BATS))            
            {
                Buff buff = character.BuffComponent.GetBuff((short)BuffVnums.SWARM_OF_BATS);
                await character.RemoveBuffAsync(false, buff);
            }
            
            if (character.TriggerAmbush)
            {
                Buff buff = character.BuffComponent.GetBuff((int)BuffVnums.AMBUSH);
                await character.RemoveBuffAsync(false, buff);
                Buff newBuff = _buffFactory.CreateBuff((int)BuffVnums.AMBUSH_RAID, character);
                await character.AddBuffAsync(newBuff);
                character.TriggerAmbush = false;
            }
            
            if (character.BCardComponent.HasBCard(BCardType.SpecialActions, (byte)AdditionalTypes.SpecialActions.Hide))
            {
                BCardDTO invisibilityState = character.BuffComponent.GetAllBuffs()
                    .SelectMany(buff => buff.BCards)
                    .FirstOrDefault(b => b.Type == (short)BCardType.SpecialActions &&
                        b.SubType == (byte)AdditionalTypes.SpecialActions.Hide);

                if (invisibilityState?.CardId is not null)
                {
                    Buff existingBuff = character.BuffComponent.GetBuff(invisibilityState.CardId.Value);
                    if (existingBuff != null)
                    {
                        character.RemoveBuffAsync(invisibilityState.CardId.Value).ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                }
            }
        }

        switch (hitType)
        {
            case HitType.Miss:
            {
                if (caster.IsPlayer())
                {
                    var character = (IPlayerEntity)caster;
                    character.LastSkillCombo = null;
                    character.Session.SendMsCPacket(0);
                    character.Session.RefreshQuicklist();
                    character.CleanComboState();
                }
                
                foreach ((int _, BCardDTO bCard) in target.BCardComponent.GetBuffBCards())
                {
                    switch (bCard.Type)
                    {
                        case (short)BCardType.KingOfTheBeast when bCard.SubType is (byte)KingOfTheBeast.HealthIncreaseOnDodge:
                        case (short)BCardType.KingOfTheBeast when bCard.SubType is (byte)KingOfTheBeast.MissingHPIncreasedOnDodge:
                        case (short)BCardType.FourthGlacernonFamilyRaid when bCard.SubType is (byte)FourthGlacernonFamilyRaid.RemoveRandomDebuff:
                        case (short)BCardType.DropItemTwice when bCard.SubType is (byte)DropItemTwice.EffectOnEnemyWhileDefendingChance:
                        case (short)BCardType.Damage when bCard.SubType is (byte)Damage.HPIncreasedEveryAttackReceived:
                        case (short)BCardType.Damage when bCard.SubType is (byte)Damage.MPIncreasedEveryAttackReceived:
                        case (short)BCardType.MysticArts when bCard.SubType is (byte)MysticArts.DodgeAndMakeChance:
                            _bCardHandlerContainer.Execute(caster, target, bCard, skill, hitMode: hitMode, damageDealt: totalDamage);
                            break;
                    }
                }
                caster.BroadcastSuPacket(target, skill, 0, hitMode, isFirst: hit.IsFirst);

                Skip(caster, skill);
                return;
            }
            case HitType.Critical:
            {
                if (target.BCardComponent.HasBCard(BCardType.ReflectDamage,
                        (byte)AdditionalTypes.ReflectDamage.ReflectDamageOnCritical))
                {
                    await ProbabilityReflection(caster, target, algorithmResult, skill, hit, true);
                    Skip(caster, skill);
                    return;
                }
            }
                break;
        }

        if (caster.IsPlayer())
        {
            var character = (IPlayerEntity)caster;
            if (algorithmResult.SoftDamageEffect && hit.IsFirst)
            {
                caster.MapInstance.Broadcast(character.GenerateEffectPacket(EffectType.BoostedAttack));
            }

            if (skill.Combos.Count != 0)
            {
                double increaseDamageByComboState = 0;
                ComboState comboState = _skillUsageManager.GetComboState(caster.Id, target.Id);
                increaseDamageByComboState = 0.05 + 0.1 * comboState.Hit;

                if (target.BCardDataComponent.MaxCriticals is null)
                {
                    totalDamage += (int)(totalDamage * increaseDamageByComboState);
                }

                comboState.Hit++;
                ComboDTO combo = skill.Combos.FirstOrDefault(s => s.Hit == comboState.Hit);
                if (combo != null)
                {
                    skill.HitAnimation = combo.Animation;
                    skill.HitEffect = combo.Effect;
                }

                if (skill.Combos.Max(s => s.Hit) <= comboState.Hit)
                {
                    _skillUsageManager.ResetComboState(caster.Id);
                }
            }
        }

        int maxCriticalHit = target.BCardComponent.GetAllBCardsInformation(BCardType.VulcanoElementBuff, (byte)AdditionalTypes.VulcanoElementBuff.CriticalDefenceTimes, target.Level).firstData;

        if (hitType == HitType.Critical && maxCriticalHit != 0 && totalDamage > maxCriticalHit)
        {
            if (target.BCardStackComponent.TryDecreaseBCardStack(((short)BCardType.VulcanoElementBuff, (byte)AdditionalTypes.VulcanoElementBuff.CriticalDefenceTimes)))
            {
                totalDamage = maxCriticalHit;
            }
        }

        if (caster.BCardComponent.HasBCard(BCardType.NoDefeatAndNoDamage, (byte)AdditionalTypes.NoDefeatAndNoDamage.NeverCauseDamage)
            || target.BCardComponent.HasBCard(BCardType.IncreaseDamageDebuffs, (byte)AdditionalTypes.IncreaseDamageDebuffs.EarnUltimatePointsOnBlock))
        {
            totalDamage = 0;
        }

        if (target.IsPlayer())
        {
            var character = (IPlayerEntity)target;
            if (character.SkillComponent.CanBeInterrupted && character.IsCastingSkill)
            {
                character.SkillComponent.CanBeInterrupted = false;
                character.SkillComponent.IsSkillInterrupted = true;
            }
        }

        // REFLECTION
        if (IsReflectionNoDamage(target))
        {
            await ReflectDamage(caster, target, algorithmResult, skill, hit);
            Skip(caster, skill);
            return;
        }

        if (IsReflectionWithDamage(target))
        {
            await ReflectDamage(caster, target, algorithmResult, skill, hit, true);
        }

        if (IsReflectionWithProbability(target))
        {
            await ProbabilityReflection(caster, target, algorithmResult, skill, hit);
            Skip(caster, skill);
        }

        if (target.BCardComponent.HasBCard(BCardType.DamageConvertingSkill, (byte)AdditionalTypes.DamageConvertingSkill.TransferInflictedDamage))
        {
            IBattleEntity receiver = _sacrificeManager.GetCaster(target);

            (int firstData, int secondData) transferBCard =
                target.BCardComponent.GetAllBCardsInformation(BCardType.DamageConvertingSkill, (byte)AdditionalTypes.DamageConvertingSkill.TransferInflictedDamage, target.Level);

            if (receiver != null && receiver.IsAlive())
            {
                int damageToDealt = (int)(totalDamage * (transferBCard.firstData * 0.01));
                int damageToDecrease = (int)(totalDamage * (transferBCard.secondData * 0.01));

                if (receiver.Hp - damageToDealt <= 0)
                {
                    receiver.Hp = 1;
                }
                else
                {
                    receiver.Hp -= damageToDealt;
                }

                receiver.BroadcastCleanSuPacket(receiver, damageToDealt, true);
                totalDamage -= damageToDecrease;
            }
        }
        
        if (skill.CastId == 0 && caster.BCardComponent.HasBCard(BCardType.TeamArenaBuff, (byte)AdditionalTypes.TeamArenaBuff.OnBasicAttackChanceProcSecondOne))
        {
            (int firstData, int secondData) basicAttackAdd = caster.BCardComponent.GetAllBCardsInformation(BCardType.TeamArenaBuff, (byte)AdditionalTypes.TeamArenaBuff.OnBasicAttackChanceProcSecondOne, target.Level);
            
            if (target != null && target.IsAlive() && _randomGenerator.RandomNumber() <= basicAttackAdd.firstData)
            {
                if (caster is IPlayerEntity playerEntity && playerEntity.Session.CurrentMapInstance.MapVnum == (int)MapIds.SNOWMAN_BOSS_ROOM)
                {
                    playerEntity.Session.SendCancelPacket(CancelType.NotInCombatMode);
                    return;
                }
                
                double damageToDealt = basicAttackAdd.secondData * 0.01;
                    
                int basicAttackDamage = (int)(totalDamage * damageToDealt);
                    
                await _eventPipeline.ProcessEventAsync(new EntityDamageEvent
                {
                    Damaged = target,
                    Damager = caster,
                    Damage = basicAttackDamage,
                    CanKill = true,
                    SkillInfo = skill
                }, cancellation);
                    
                caster.BroadcastSuPacket(target, skill, basicAttackDamage, hitMode, true);
            }
        }

        // TODO: Rework this and create a yaml config file. - Dazynnn
        if (target.IsMonster())
        {
            IMapInstance mapInstance = caster?.MapInstance;
            IMonsterEntity monster = mapInstance?.GetMonsterById(target.Id);

            if (monster != null)
            {
                switch (monster.MonsterVNum)
                {
                   case 388: // Chicken King
                {
                    totalDamage = 200;
                }
                    break;

                case 774: // Chicken Queen
                {
                    totalDamage = 388;
                }
                    break;

                case 2317: // Mad March Hare
                {
                    totalDamage = 175;
                }
                    break;

                case 2332: // Imp Cheongbi
                {
                    totalDamage = 507;
                }
                    break;

                case 452: // Grasslin Level 25
                {
                    totalDamage = 1003;
                }
                    break;

                case 450: // Grasslin Level 30
                {
                    totalDamage = 334;
                }
                    break;

                case 1500: // Captain Pete O'Peng
                {
                    totalDamage = 338;
                }
                    break;

                case 2357: // Lola Lopears
                {
                    totalDamage = 193;
                }
                    break;

                case 1381: // Jack O'Lantern
                {
                    totalDamage = 600;
                }
                    break;

                case 2309: // Foxy
                {
                    totalDamage = 193;
                }
                    break;

                case 533: // Huge Snowman Head
                {
                    totalDamage = 63;
                }
                    break;

                case 2316: // Maru
                {
                    totalDamage = 193;
                }
                    break;

                case 2639: // Yertirand
                {
                    totalDamage = 666;
                }
                    break;

                case 2619: // Fafnir
                {
                    totalDamage = 362;
                }
                    break;

                case 839: // Bushi King
                {
                    totalDamage = 644;
                }
                    break;

                case 2662: // Bone Drake
                {
                    totalDamage = 1666;
                }
                    break;
                }
            }
        }

        int damageDivided = caster.MapInstance.MapInstanceType switch
        {
            MapInstanceType.Icebreaker => 3,
            MapInstanceType.RainbowBattle => 3,
            MapInstanceType.TalentArena => 3,
            MapInstanceType.ArenaInstance => 2,
            _ => 1
        };
        
        if (caster.MapInstance.HasMapFlag(MapFlags.ACT_4))
        {
            damageDivided = 2;
        }

        await _eventPipeline.ProcessEventAsync(new EntityDamageEvent
        {
            Damaged = target,
            Damager = caster,
            Damage = totalDamage,
            CanKill = true,
            SkillInfo = skill
        });

        caster.BroadcastSuPacket(target, skill, totalDamage / damageDivided, hitMode, isFirst: hit.IsFirst);
        
        if (hit.IsFirst)
        {
            hit.IsFirst = false;
        }

        if (target.IsAlive())
        {
            foreach (BCardDTO bCard in afterAttackAllTargets)
            {
                _bCardHandlerContainer.Execute(target, caster, bCard, skill);
            }
            
            foreach ((int _, BCardDTO bCard) in caster.BCardComponent.GetBuffBCards())
            {
                switch (bCard.Type)
                {
                    case (short)BCardType.IncreaseSpPoints when bCard.SubType is (byte)AdditionalTypes.IncreaseSpPoints.SpCardAttackPointIncrease:
                    case (short)BCardType.IncreaseSpPoints when bCard.SubType is (byte)AdditionalTypes.IncreaseSpPoints.SpCardDefensePointIncrease:
                    case (short)BCardType.IncreaseSpPoints when bCard.SubType is (byte)AdditionalTypes.IncreaseSpPoints.SpCardElementPointIncrease:
                    case (short)BCardType.IncreaseSpPoints when bCard.SubType is (byte)AdditionalTypes.IncreaseSpPoints.SpCardHpMpPointIncrease:
                    case (short)BCardType.IncreaseSpPoints when bCard.SubType is (byte)AdditionalTypes.SPCardUpgrade.IncreaseAllSkillPoints:
                    case (short)BCardType.StealBuff when bCard.SubType is (byte)AdditionalTypes.StealBuff.ChanceSummonOnyxDragon:
                    case (short)BCardType.DragonSkills when bCard.SubType is (byte)AdditionalTypes.DragonSkills.MagicArrowChance:
                    case (short)BCardType.Tattoo when bCard.SubType is (byte)AdditionalTypes.Tattoo.OnAttackProcLionLoa:
                    case (short)BCardType.Tattoo when bCard.SubType is (byte)AdditionalTypes.Tattoo.OnAttackProcEagleLoa:
                    case (short)BCardType.Tattoo when bCard.SubType is (byte)AdditionalTypes.Tattoo.OnAttackProcSnakeLoa:
                    case (short)BCardType.Tattoo when bCard.SubType is (byte)AdditionalTypes.Tattoo.OnAttackProcBearLoa:
                    case (short)BCardType.Quest when bCard.SubType is (byte)AdditionalTypes.Quest.HealHpByInflictedDamages:
                    case (short)BCardType.Quest when bCard.SubType is (byte)AdditionalTypes.Quest.HealMpByInflictedDamages:
                    case (short)BCardType.SniperAttack when bCard.SubType is (byte)AdditionalTypes.SniperAttack.ChanceCausing:
                    case (short)BCardType.Reflection when bCard.SubType is (byte)AdditionalTypes.Reflection.HPIncreased:
                    case (short)BCardType.Reflection when bCard.SubType is (byte)AdditionalTypes.Reflection.MPIncreased:
                    case (short)BCardType.FourthGlacernonFamilyRaid when bCard.SubType is (byte)AdditionalTypes.FourthGlacernonFamilyRaid.HPIncreasedDamageGiven:
                    case (short)BCardType.DropItemTwice when bCard.SubType is (byte)AdditionalTypes.DropItemTwice.EffectOnEnemyWhileAttackingChance:
                    case(short) BCardType.MineralTokenEffects when bCard.SubType is (byte)AdditionalTypes.MineralTokenEffects.AttackChanceToTriggerEffect:
                    case (short)BCardType.LordCalvinas when bCard.SubType is (byte)AdditionalTypes.LordCalvinas.SpawnNeutralDragon:
                    case (short)BCardType.LordCalvinas when bCard.SubType is (byte)AdditionalTypes.LordCalvinas.SpawnFireDragon:
                    case (short)BCardType.LordCalvinas when bCard.SubType is (byte)AdditionalTypes.LordCalvinas.SpawnIceDragon:
                    case (short)BCardType.LordCalvinas when bCard.SubType is (byte)AdditionalTypes.LordCalvinas.SpawnMoonDragon:
                    case (short)BCardType.SESpecialist when bCard.SubType is (byte)AdditionalTypes.SESpecialist.SpawnSkyDragon:
                        _bCardHandlerContainer.Execute(target, caster, bCard, skill, hitMode: hitMode, damageDealt: totalDamage);
                        break;
                }
            }

            foreach ((int _, BCardDTO bCard) in target.BCardComponent.GetBuffBCards())
            {
                switch (bCard.Type)
                {
                    case (short)BCardType.BlasterHeat when bCard.SubType is (byte)AdditionalTypes.BlasterHeat.ReduceOpponentHpWithHighHeatingEffect:
                    case (short)BCardType.VoodooPriest when bCard.SubType is (byte)AdditionalTypes.VoodooPriest.TakeDamageXTimesBuffDisappear:
                    case (short)BCardType.InflictSkill when bCard.SubType is (byte)AdditionalTypes.InflictSkill.OnDefenceResetCooldownOfSkillUsed:
                    case (short)BCardType.FuelHeatPoint when bCard.SubType is (byte)AdditionalTypes.FuelHeatPoint.ConsumeFuelPointsChanceToResetCooldown:
                    case (short)BCardType.LordHatus when bCard.SubType is (byte)AdditionalTypes.LordHatus.RestoreHpByDamageTaken:
                    case (short)BCardType.LordHatus when bCard.SubType is (byte)AdditionalTypes.LordHatus.RestoreMpByDamageTaken:
                    case (short)BCardType.SecondSPCard when bCard.SubType is (byte)AdditionalTypes.SecondSPCard.HitAttacker:
                    case (short)BCardType.Reflection when bCard.SubType is (byte)AdditionalTypes.Reflection.EnemyHPIncreased:
                    case (short)BCardType.Reflection when bCard.SubType is (byte)AdditionalTypes.Reflection.EnemyHPDecreased:
                    case (short)BCardType.FourthGlacernonFamilyRaid when bCard.SubType is (byte)AdditionalTypes.FourthGlacernonFamilyRaid.RemoveRandomDebuff:
                    case (short)BCardType.RecoveryAndDamagePercent when bCard.SubType is (byte)AdditionalTypes.RecoveryAndDamagePercent.MPRecovered:
                    case (short)BCardType.DropItemTwice when bCard.SubType is (byte)AdditionalTypes.DropItemTwice.EffectOnEnemyWhileDefendingChance:
                    case (short)BCardType.DropItemTwice when bCard.SubType is (byte)AdditionalTypes.DropItemTwice.EffectOnEnemyWhileDefendingChance:
                        _bCardHandlerContainer.Execute(caster, target, bCard, skill, hitMode: hitMode, damageDealt: totalDamage);
                        break;
                }
            }
        }

        if (!target.IsAlive() && caster.IsPlayer())
        {
            (caster as IPlayerEntity).Session.SendCancelPacket(CancelType.NotInCombatMode);
        }

        switch (skill.Vnum)
        {
            case (short)SkillsVnums.HOLY_EXPLOSION when target.BuffComponent.HasBuff((short)BuffVnums.ILLUMINATING_POWDER):
            {
                await _eventPipeline.ProcessEventAsync(new EntityDamageEvent
                {
                    Damaged = target,
                    Damager = caster,
                    Damage = totalDamage,
                    CanKill = true,
                    SkillInfo = skill
                });

                caster.BroadcastSuPacket(target, skill, totalDamage / damageDivided, hitMode, true);
                Buff buff = target.BuffComponent.GetBuff((short)BuffVnums.ILLUMINATING_POWDER);
                await target.RemoveBuffAsync(false, buff);
                break;
            }
            case (short)SkillsVnums.CONVERT when target.BuffComponent.HasBuff((short)BuffVnums.CORRUPTION):
            {
                int convertDamage = totalDamage / 2;
                await _eventPipeline.ProcessEventAsync(new EntityDamageEvent
                {
                    Damaged = target,
                    Damager = caster,
                    Damage = convertDamage,
                    CanKill = true,
                    SkillInfo = skill
                });

                caster.BroadcastSuPacket(target, skill, convertDamage / damageDivided, hitMode, true);
                Buff buff = target.BuffComponent.GetBuff((short)BuffVnums.CORRUPTION);
                await target.RemoveBuffAsync(false, buff);
                break;
            }
        }
    }

    private void Skip(IBattleEntity entity, SkillInfo skillInfo)
    {
        if (!entity.IsPlayer())
        {
            return;
        }

        if (!skillInfo.Combos.Any())
        {
            return;
        }

        _skillUsageManager.ResetComboState(entity.Id);
    }

    private static bool IsReflectionNoDamage(IBattleEntity target) =>
        target.BCardComponent.HasBCard(BCardType.TauntSkill, (byte)AdditionalTypes.TauntSkill.ReflectsMaximumDamageFromNegated) ||
        target.BCardComponent.HasBCard(BCardType.TauntSkill, (byte)AdditionalTypes.TauntSkill.ReflectsMaximumDamageFrom);

    private static bool IsReflectionWithDamage(IBattleEntity target) =>
        target.BCardComponent.HasBCard(BCardType.DamageConvertingSkill, (byte)AdditionalTypes.DamageConvertingSkill.ReflectMaximumReceivedDamage);

    private static bool IsReflectionWithProbability(IBattleEntity target) =>
        target.BCardComponent.HasBCard(BCardType.DealDamageAround, (byte)AdditionalTypes.DealDamageAround.DamageReflect);

    private async Task ProbabilityReflection(IBattleEntity caster, IBattleEntity target, DamageAlgorithmResult damageAlgorithmResult,
        SkillInfo skill, HitInformation hitInformation, bool isCriticalBCard = false)
    {
        int randomNumber = _randomGenerator.RandomNumber();

        if (isCriticalBCard)
        {
            (int firstData, int secondData) = target.BCardComponent.GetAllBCardsInformation(BCardType.ReflectDamage,
                (byte)AdditionalTypes.ReflectDamage.ReflectDamageOnCritical, target.Level);

            if (randomNumber > firstData)
            {
                return;
            }

            int totalDamage = (int)(damageAlgorithmResult.Damages * secondData * 0.01);

            HitType hitType = damageAlgorithmResult.HitType;

            await _eventPipeline.ProcessEventAsync(new EntityDamageEvent
            {
                Damaged = caster,
                Damager = target,
                Damage = totalDamage,
                CanKill = false,
                SkillInfo = skill
            });

            SuPacketHitMode hitMode = GetHitMode(skill, hitType, hitInformation.IsFirst);
            caster.BroadcastDamage(totalDamage, DmType.Reflect);
            target.BroadcastSuPacket(caster, skill, totalDamage, hitMode, true, hitInformation.IsFirst);
        }
        else
        {
            (int firstData, int secondData) = target.BCardComponent.GetAllBCardsInformation(BCardType.DealDamageAround,
                (byte)AdditionalTypes.DealDamageAround.DamageReflect, target.Level);

            if (randomNumber > firstData)
            {
                return;
            }

            int totalDamage = (int)(damageAlgorithmResult.Damages * secondData * 0.01);

            HitType hitType = damageAlgorithmResult.HitType;

            await _eventPipeline.ProcessEventAsync(new EntityDamageEvent
            {
                Damaged = caster,
                Damager = target,
                Damage = totalDamage,
                CanKill = false,
                SkillInfo = skill
            });

            caster.BroadcastDamage(totalDamage, DmType.Reflect);
            SuPacketHitMode hitMode = GetHitMode(skill, hitType, hitInformation.IsFirst);
            target.BroadcastSuPacket(caster, skill, totalDamage, hitMode, true, hitInformation.IsFirst);
        }
    }

    private async Task ReflectDamage(IBattleEntity caster, IBattleEntity target, DamageAlgorithmResult damageAlgorithmResult, SkillInfo skill, HitInformation hitInformation,
        bool shouldDamageCaster = false)
    {
        int totalDamage = damageAlgorithmResult.Damages;
        HitType hitType = damageAlgorithmResult.HitType;

        (int firstData, int secondData) reflection =
            target.BCardComponent.GetAllBCardsInformation(BCardType.TauntSkill, (byte)AdditionalTypes.TauntSkill.ReflectsMaximumDamageFromNegated, target.Level);
        if (reflection.firstData == 0)
        {
            reflection = target.BCardComponent.GetAllBCardsInformation(BCardType.TauntSkill, (byte)AdditionalTypes.TauntSkill.ReflectsMaximumDamageFrom, target.Level);
        }

        if (shouldDamageCaster)
        {
            reflection = target.BCardComponent.GetAllBCardsInformation(BCardType.DamageConvertingSkill, (byte)AdditionalTypes.DamageConvertingSkill.ReflectMaximumReceivedDamage, target.Level);
        }

        if (totalDamage > reflection.firstData)
        {
            totalDamage = reflection.firstData;
        }

        await _eventPipeline.ProcessEventAsync(new EntityDamageEvent
        {
            Damaged = caster,
            Damager = target,
            Damage = totalDamage,
            CanKill = false,
            SkillInfo = skill
        });

        SuPacketHitMode hitMode = GetHitMode(skill, hitType, hitInformation.IsFirst);
        target.BroadcastSuPacket(caster, skill, totalDamage, hitMode, true, hitInformation.IsFirst);

        if (!shouldDamageCaster)
        {
            if (skill.Vnum != (short)SkillsVnums.DOUBLE_RIPPER)
            {
                SuPacketHitMode reflectHitMode = skill.AoERange != 0 ? SuPacketHitMode.ReflectionAoeMiss : SuPacketHitMode.Miss;
                caster.BroadcastSuPacket(caster, skill, totalDamage, reflectHitMode, false, hitInformation.IsFirst); // Yes, it should be false for reflect.
            }

            if (hitInformation.IsFirst)
            {
                hitInformation.IsFirst = false;
            }
        }
    }

    private SuPacketHitMode GetHitMode(SkillInfo skill, HitType hitType, bool isFirst)
    {
        if (skill.TargetType == TargetType.Self && (skill.HitType == TargetHitType.EnemiesInAffectedAoE || skill.HitType == TargetHitType.AlliesInAffectedAoE))
        {
            switch (hitType)
            {
                case HitType.Miss:
                    return SuPacketHitMode.MissAoe;
                case HitType.Normal:
                    return SuPacketHitMode.AttackedInAoe;
                case HitType.Critical:
                    return SuPacketHitMode.AttackedInAoeCrit;
            }
        }

        if (isFirst)
        {
            switch (hitType)
            {
                case HitType.Miss:
                    return SuPacketHitMode.Miss;
                case HitType.Normal:
                    return skill.TargetType == TargetType.NonTarget ? SuPacketHitMode.AttackedInAoe : SuPacketHitMode.SuccessAttack;
                case HitType.Critical:
                    return SuPacketHitMode.CriticalAttack;
            }
        }
        else
        {
            switch (hitType)
            {
                case HitType.Miss:
                    return SuPacketHitMode.MissAoe;
                case HitType.Normal:
                    return SuPacketHitMode.AttackedInAoe;
                case HitType.Critical:
                    return SuPacketHitMode.AttackedInAoeCrit;
            }
        }

        return SuPacketHitMode.SuccessAttack;
    }
}