using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using Plugin.Raids.Extension;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game;
using WingsEmu.Game._ECS.Systems;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Battle.Event;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Buffs.Events;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Game.RainbowBattle;
using WingsEmu.Game.Skills;
using WingsEmu.Game.Triggers;
using WingsEmu.Packets.Enums;
using static WingsEmu.Packets.Enums.AdditionalTypes;
using Buff = WingsEmu.Game.Buffs.Buff;

namespace NosEmu.Plugins.BasicImplementations.Event.Battle;

public class EntityDamageEventHandler : IAsyncEventProcessor<EntityDamageEvent>
{
    private static readonly HashSet<BuffVnums> _meditationBuffs = new() { BuffVnums.SPIRIT_OF_STRENGTH, BuffVnums.SPIRIT_OF_TEMPERANCE, BuffVnums.SPIRIT_OF_ENLIGHTENMENT };
    private readonly IBCardEffectHandlerContainer _bCardEffectHandlerContainer;
    private readonly IBuffFactory _buff;
    private readonly IBuffFactory _buffFactory;
    private readonly IBuffsToRemoveConfig _buffsToRemoveConfig;
    private readonly IGameLanguageService _gameLanguage;
    private readonly GameRevivalConfiguration _gameRevivalConfiguration;
    private readonly IMeditationManager _meditationManager;
    private readonly GameMinMaxConfiguration _minMaxConfiguration;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IAsyncEventPipeline _asyncEventPipeline;
    private readonly RainbowBattleConfiguration _rainbowBattleConfiguration;

    public EntityDamageEventHandler(IBuffFactory buff, IMeditationManager meditationManager, IRandomGenerator randomGenerator, IBuffFactory buffFactory,
        GameRevivalConfiguration gameRevivalConfiguration, IBuffsToRemoveConfig buffsToRemoveConfig, IBCardEffectHandlerContainer bCardEffectHandlerContainer,
        GameMinMaxConfiguration minMaxConfiguration, IGameLanguageService gameLanguage, IAsyncEventPipeline asyncEventPipeline, RainbowBattleConfiguration rainbowBattleConfiguration)
    {
        _buff = buff;
        _meditationManager = meditationManager;
        _randomGenerator = randomGenerator;
        _buffFactory = buffFactory;
        _gameRevivalConfiguration = gameRevivalConfiguration;
        _buffsToRemoveConfig = buffsToRemoveConfig;
        _bCardEffectHandlerContainer = bCardEffectHandlerContainer;
        _minMaxConfiguration = minMaxConfiguration;
        _gameLanguage = gameLanguage;
        _asyncEventPipeline = asyncEventPipeline;
        _rainbowBattleConfiguration = rainbowBattleConfiguration;
    }

    public async Task HandleAsync(EntityDamageEvent e, CancellationToken cancellation)
    {
        IBattleEntity defender = e.Damaged;
        IBattleEntity attacker = e.Damager;
        int damage = e.Damage;
        int initialDefenderHp = defender.Hp;
        IMapInstance map = defender.MapInstance;
        SkillInfo skillInfo = e.SkillInfo;
        bool shouldDamageNamaju = defender is IMonsterEntity { DamagedOnlyLastJajamaruSkill: true } && skillInfo.Vnum == (short)SkillsVnums.JAJAMARU_LAST_SKILL;

        if (!defender.IsAlive())
        {
            return;
        }

        if (map == null)
        {
            return;
        }

        /* Sorry, it need to be hardcoded :c */
        await RemoveDamagedHardcodedBuff(defender, skillInfo);
        await RemoveDamagerHardcodedBuff(attacker);
        await RemovePvPHardcodedBuff(attacker, defender);
        await TrainerSpecialistMateLevelUp(attacker, defender);
        await MateUpgradeProgress(attacker, defender);
        VoodooIncreaseDamagePerDebuff(defender, skillInfo, ref damage);
        StoreVoodooDamage(attacker, skillInfo, damage);
        ExecuteVoodooDamageStored(attacker, skillInfo, ref damage);
        TryLoseLoyalty(attacker, defender);

        if (defender.BCardComponent.HasBCard(BCardType.Drain, (byte)AdditionalTypes.Drain.AttackedBySunWolfIncreaseEffectDuration))
        {
            (int firstData, int secondData) = defender.BCardComponent.GetAllBCardsInformation(BCardType.Drain, (byte)Drain.AttackedBySunWolfIncreaseEffectDuration, defender.Level);

            if (attacker is IMateEntity { NpcMonsterVNum: (short)MonsterVnum.SUN_WOLF } && attacker.IsMate())
            {
                if (!defender.IsSucceededChance(firstData) || defender is not IPlayerEntity playerEntity)
                {
                    return;
                }

                if (playerEntity.BCardDataComponent.SunWolfChanceIncreaseBuffDuration < secondData)
                {
                    Buff toIncrease = playerEntity.BuffComponent.GetAllBuffs().FirstOrDefault(buff => buff.BCards.Any(b => b.Type == (byte)BCardType.Drain && b.SubType == (byte)Drain.AttackedBySunWolfIncreaseEffectDuration));

                    if (toIncrease is not null)
                    {
                        int remainingTimeInMilliseconds = toIncrease.RemainingTimeInMilliseconds();
                        var additionalDuration = TimeSpan.FromSeconds(2);
                        toIncrease.Duration = additionalDuration + TimeSpan.FromMilliseconds(remainingTimeInMilliseconds);
                        toIncrease.SetBuffDuration(toIncrease.Duration);

                        playerEntity.SendBfPacket(toIncrease, 0);
                        playerEntity.BCardDataComponent.SunWolfChanceIncreaseBuffDuration++;
                    }
                }
            }
        }
        
        if (defender.BCardComponent.HasBCard(BCardType.Drain, (byte)AdditionalTypes.Drain.AttackedBySunWolfChanceCast))
        {
            (int firstData, int secondData) = defender.BCardComponent.GetAllBCardsInformation(BCardType.Drain,
                (byte)AdditionalTypes.Drain.AttackedBySunWolfChanceCast, defender.Level);
            if (attacker.IsMate())
            {
                var sunWolf = (IMateEntity)attacker;
                if (sunWolf.NpcMonsterVNum is (short)MonsterVnum.SUN_WOLF)
                {
                    if (!defender.IsSucceededChance(firstData))
                    {
                        return;
                    }
                    
                    Buff buff = _buff.CreateBuff(secondData, attacker);
                    await defender.AddBuffAsync(buff);
                }
            }
        }
        

        if (attacker is IPlayerEntity player && damage != 0)
        {
            if (player.IsInRaidParty)
            {
                if (defender is IMonsterEntity { IsBoss: true })
                {
                    player.HardcoreComponent.TotalRaidDamage += damage;
                }
            }

            switch (player.MapInstance?.MapInstanceType)
            {
                case MapInstanceType.EventGameInstance:
                    player.InstantCombatDamage += damage;
                    break;
            }
        }

        if (defender.BuffComponent.HasBuff((int)BuffVnums.BLOCK))
        {
            if (defender.IsPlayer())
            {
                var masterWolf = (IPlayerEntity)defender;
                masterWolf.UpdateEnergyBar(1000).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            return;
        }

        if (attacker.BCardComponent.HasBCard(BCardType.BearSpirit, (byte)AdditionalTypes.BearSpirit.DamageNextSkillIncreased) && damage != 0)
        {
            // Need to verify on official.
            if (skillInfo.CastId == 0)
            {
                return;
            }

            (int firstData, int secondData) = attacker.BCardComponent.GetAllBCardsInformation(BCardType.BearSpirit,
                (byte)AdditionalTypes.BearSpirit.DamageNextSkillIncreased, attacker.Level);

            if (attacker.BCardDataComponent.VoodooDamageStored.HasValue)
            {
                attacker.BCardDataComponent.VoodooDamageStored = Math.Clamp(damage, firstData, secondData);
            }
        }
        
        if (skillInfo.BCards.Any(x => x.Type == (short)BCardType.VoodooPriest && x.SubType ==
                (byte)AdditionalTypes.VoodooPriest.IncreaseFixedDamageDebuffStack) && damage != 0)
        {
            BCardDTO bCardSkill = skillInfo.BCards.FirstOrDefault(x => x.Type == (short)BCardType.VoodooPriest && x.SubType ==
                (byte)AdditionalTypes.VoodooPriest.IncreaseFixedDamageDebuffStack);

            IReadOnlyList<Buff> debuffs = defender.BuffComponent.GetAllBuffs(x => x.BuffGroup == BuffGroup.Bad);

            if (bCardSkill != null)
            {
                damage += bCardSkill.FirstData * debuffs.Count;
                damage = Math.Min(damage, bCardSkill.SecondData);
            }
        }
        
        if (skillInfo.BCards.Any(x => x.Type == (short)BCardType.VoodooPriest && x.SubType ==
                (byte)AdditionalTypes.VoodooPriest.IncreaseFixedDamageBuffStack) && damage != 0)
        {
            BCardDTO bCardSkill = skillInfo.BCards.FirstOrDefault(x => x.Type == (short)BCardType.VoodooPriest && x.SubType ==
                (byte)AdditionalTypes.VoodooPriest.IncreaseFixedDamageBuffStack);
            
            IReadOnlyList<Buff> debuffs = defender.BuffComponent.GetAllBuffs(x => x.BuffGroup == BuffGroup.Good);
            
            if (bCardSkill != null)
            {
                damage += bCardSkill.FirstData * debuffs.Count;
                damage = Math.Min(damage, bCardSkill.SecondData);
            }
        }

        if (attacker.BCardComponent.HasBCard(BCardType.SpecialDamageAndExplosions, (byte)AdditionalTypes.SpecialDamageAndExplosions.EffectRemovedAfterAttack))
        {
            int secondData = attacker.BCardComponent.GetAllBCardsInformation(BCardType.SpecialDamageAndExplosions,
                (byte)AdditionalTypes.SpecialDamageAndExplosions.EffectRemovedAfterAttack, attacker.Level).secondData;
            await attacker.RemoveBuffAsync(secondData);
        }
        
        if (attacker.BCardComponent.HasBCard(BCardType.LordBerios, (byte)AdditionalTypes.LordBerios.HpReducedOnAttackSkill) && skillInfo.CastId != 0)
        {
            int firstData = attacker.BCardComponent.GetAllBCardsInformation(BCardType.LordBerios,
                (byte)AdditionalTypes.LordBerios.HpReducedOnAttackSkill, attacker.Level).firstData;

            int damagePenalty = (int)(attacker.Hp * (firstData * 0.01));
            if (attacker.Hp - damagePenalty <= 1)
            {
                damagePenalty = Math.Abs(attacker.Hp - 1);
            }

            attacker.Hp -= damagePenalty;
            if (damagePenalty == 0)
            {
                return;
            }

            attacker.BroadcastDamage(damagePenalty);
        }


        switch (defender)
        {
            case IPlayerEntity c:
                if (c.IsSeal)
                {
                    return;
                }

                if (c.TriggerAmbush)
                {
                    break;
                }

                if (c.BCardComponent.HasBCard(BCardType.NoDefeatAndNoDamage, (byte)AdditionalTypes.NoDefeatAndNoDamage.DecreaseHPNoDeath) ||
                    c.BCardComponent.HasBCard(BCardType.DamageInflict, (byte)AdditionalTypes.DamageInflict.DecreaseHpNoDeath))
                {
                    e.CanKill = false;
                }

                if (!c.BCardComponent.HasBCard(BCardType.LordBerios, (byte)AdditionalTypes.LordBerios.InvisibleStateUnchangedOnDefence))
                {
                    await c.RemoveInvisibility();
                }
                break;
            case INpcEntity { HasGodMode: true }:
                return;
            case IMateEntity mate:
                if (mate.BCardComponent.HasBCard(BCardType.NoDefeatAndNoDamage, (byte)AdditionalTypes.NoDefeatAndNoDamage.DecreaseHPNoDeath) ||
                    mate.BCardComponent.HasBCard(BCardType.DamageInflict, (byte)AdditionalTypes.DamageInflict.DecreaseHpNoDeath))
                {
                    e.CanKill = false;
                }
                break;
            case IMonsterEntity monster:
                if (monster.BCardComponent.HasBCard(BCardType.DamageInflict, (byte)AdditionalTypes.DamageInflict.DecreaseHpNoDeath) ||
                    monster.BCardComponent.HasBCard(BCardType.NoDefeatAndNoDamage, (byte)AdditionalTypes.NoDefeatAndNoDamage.DecreaseHPNoDeath))
                {
                    e.CanKill = false;
                }

                if (monster.BCardComponent.HasBCard(BCardType.TimeCircleSkills, (byte)AdditionalTypes.TimeCircleSkills.DisableHPConsumption))
                {
                    return;
                }

                if (monster.BCardComponent.HasBCard(BCardType.FourthGlacernonFamilyRaid, (byte)AdditionalTypes.FourthGlacernonFamilyRaid.DisableHPMPConsumption))
                {
                    return;
                }

                if (monster.DamagedOnlyLastJajamaruSkill && !shouldDamageNamaju)
                {
                    return;
                }

                if (monster.OnFirstDamageReceive && monster.BCards.Any())
                {
                    foreach (BCardDTO bCard in monster.BCards.Where(x => x.TriggerType is BCardNpcMonsterTriggerType.ON_FIRST_ATTACK))
                    {
                        _bCardEffectHandlerContainer.Execute(monster, monster, bCard);
                    }

                    monster.OnFirstDamageReceive = false;
                }

                break;
        }

        if (defender.HasGodMode())
        {
            return;
        }

        switch (attacker)
        {
            case IPlayerEntity { Morph: (int)MorphType.PetTrainer or (int)MorphType.PetTrainerSkin }:
                e.CanKill = false;
                break;
            case IPlayerEntity playerEntity when playerEntity.BCardComponent.HasBCard(BCardType.DamageInflict, (byte)AdditionalTypes.DamageInflict.DecreaseHpNoKill) ||
            playerEntity.BCardComponent.HasBCard(BCardType.NoDefeatAndNoDamage, (byte)AdditionalTypes.NoDefeatAndNoDamage.DecreaseHPNoKill):
                e.CanKill = false;
                break;
            case IMateEntity mate when mate.BCardComponent.HasBCard(BCardType.DamageInflict, (byte)AdditionalTypes.DamageInflict.DecreaseHpNoKill) ||
            mate.BCardComponent.HasBCard(BCardType.NoDefeatAndNoDamage, (byte)AdditionalTypes.NoDefeatAndNoDamage.DecreaseHPNoKill):
                e.CanKill = false;
                break;
            case IMonsterEntity act4Monster when act4Monster.BCardComponent.HasBCard(BCardType.DamageInflict, (byte)AdditionalTypes.DamageInflict.DecreaseHpNoKill) ||
                act4Monster.BCardComponent.HasBCard(BCardType.NoDefeatAndNoDamage, (byte)AdditionalTypes.NoDefeatAndNoDamage.DecreaseHPNoKill) ||
                act4Monster.BCardComponent.HasBCard(BCardType.NoDefeatAndNoDamage, (byte)AdditionalTypes.NoDefeatAndNoDamage.DecreaseHPNoDeath):
                e.CanKill = false;
                break;
        }

        damage /= map.MapInstanceType switch
        {
            MapInstanceType.Icebreaker => 3,
            MapInstanceType.RainbowBattle => 3,
            MapInstanceType.ArenaInstance => 2,
            _ => 1
        };
        
        if (map.IsAct6PvpInstance)
        {
            damage /= 2;
        }
        
        if (map.HasMapFlag(MapFlags.ACT_4))
        {
            damage /= 2;
        }
        
        if (attacker is IMonsterEntity { MonsterVNum: (int)MonsterVnum.ONYX_MONSTER })
        {
            damage /= 2;
        }

        if (shouldDamageNamaju)
        {
            damage = GenerateNamajuDamage(defender as IMonsterEntity);
        }
        
        ProcessBuffDamage(defender, damage);
        
        MpIncreasedByDmg(attacker, defender, damage);
        MpDecreasedByDmg(attacker, defender, damage);
        
        ReduceAbsorptionShellDmg(defender, ref damage);

        // Heal x% of inflicted damage by reducing MP.
        ReduceDamageByMp(defender, ref damage);
        
        IncreaseDamageByMarkedBuff(defender, ref damage);
        IncreaseDamageByMarkedBuff2(defender, ref damage);
        
        IncreaseUltimateSkill(attacker, skillInfo, ref damage);

        await ApplyBuffWhileBlock(defender);
        await HealDefenderByGivenDamage(defender, damage);
        await ApplyDebuffEffectWhileShadowSkillElemTriggered(attacker, defender, skillInfo);
        await DecreaseHealByGivenDamage(attacker, damage);
        await HpLostWhileCasting(attacker);
        await OpportunityToAttack(defender);

        if (defender is IPlayerEntity { AdditionalHp: > 0 } characterDamaged)
        {
            int removedAdditionalHp;
            if (characterDamaged.AdditionalHp > damage)
            {
                removedAdditionalHp = damage;
            }
            else
            {
                removedAdditionalHp = characterDamaged.AdditionalHp;

                int overflow = Math.Abs(characterDamaged.AdditionalHp - damage);

                if (e.CanKill)
                {
                    if (!await attacker.ShouldSaveDefender(defender, overflow))
                    {
                        defender.Hp = overflow >= defender.Hp ? 0 : defender.Hp - overflow;
                    }
                }
                else
                {
                    defender.Hp = overflow >= defender.Hp ? 1 : defender.Hp - overflow;
                }
            }

            await characterDamaged.Session.EmitEventAsync(new RemoveAdditionalHpMpEvent
            {
                Hp = removedAdditionalHp
            });
        }
        else
        {
            if (e.CanKill)
            {
                if (!await attacker.ShouldSaveDefender(defender, damage))
                {
                    defender.Hp = damage >= defender.Hp ? 0 : defender.Hp - damage;
                }
            }
            else
            {
                defender.Hp = damage >= defender.Hp ? 1 : defender.Hp - damage;
            }
        }

        AddPlayerDamageToMonster(attacker, defender, damage);
        AddMonsterHitsToPlayer(attacker, defender);
        attacker.ApplyAttackBCard(defender, e.SkillInfo, _bCardEffectHandlerContainer);
        defender.ApplyDefenderBCard(attacker, e.SkillInfo, _bCardEffectHandlerContainer);

        BCardDTO bCardOnDeath = skillInfo.BCards.FirstOrDefault(x => x.Type == (short)BCardType.TauntSkill && x.SubType == (byte)AdditionalTypes.TauntSkill.EffectOnKill);
        if (defender.Hp <= 0 && bCardOnDeath != null && attacker.IsAlive())
        {
            Buff buffForWinner = _buff.CreateBuff(bCardOnDeath.SecondData, attacker);
            await attacker.AddBuffAsync(buffForWinner);
        }
        
        if (attacker.IsAlive() && attacker.IsMate() && 
            (attacker as IMateEntity).MateType == MateType.Partner && 
            defender.IsAlive())
        {
            int stealBuff = attacker.BCardComponent.GetAllBCardsInformation(BCardType.StealBuff, (byte)AdditionalTypes.StealBuff.StealGoodEffect, attacker.Level).firstData;
            if (stealBuff != 0)
            {
                IReadOnlyList<Buff> defenderGoofBuffUnderLevel = defender.BuffComponent.GetAllBuffs(x => x.Level <= stealBuff && x.BuffGroup == BuffGroup.Good && x.IsNormal());
                if (defenderGoofBuffUnderLevel.Count != 0)
                {
                    int getRandomOne = _randomGenerator.RandomNumber(defenderGoofBuffUnderLevel.Count);
                    Buff andFromListNow = defenderGoofBuffUnderLevel[getRandomOne];
                    await defender.RemoveBuffAsync(andFromListNow.CardId);
                    IEnumerable<IBattleEntity> allyInRange = attacker.GetAlliesInRange(attacker, 4);

                    Buff buff = _buff.CreateBuff(andFromListNow.CardId, attacker);

                    foreach (IBattleEntity ally in allyInRange)
                    {
                        await ally.AddBuffAsync(buff);
                    }
                    await attacker.AddBuffAsync(buff);
                }
            }
        }
        
        if (attacker.IsPlayer() && attacker.MapInstance.MapInstanceType == MapInstanceType.Caligor && 
            defender.IsMonster() && (defender as INpcMonsterEntity).MonsterVNum == 2305)
        {
            var playerAttacker = (attacker as IPlayerEntity);
            await playerAttacker.Session.EmitEventAsync(new Act4CaligorAddFactionPointEvent(playerAttacker.Faction, damage));
        }

        var configs = new List<MonsterHpThresholdConfig>
        {
            new MonsterHpThresholdConfig
            {
                MonsterVNum = (int)MonsterVnum.TWISTED_BEAST_KING_CARNO,
                HpThresholds = new List<HpThresholdConfig>
                {
                    new HpThresholdConfig
                    {
                        HpPercentage = 0.90f,
                        SkillConditions = new List<SkillMonsterCondition>
                        {
                            new SkillMonsterCondition { SkillVNum = SkillsVnums.CARNO_JUMP, MonsterVNum = MonsterVnum.TWISTED_BEAST_KING_CARNO }
                        },
                        Trigger = BattleTriggers.OnNinetyPercentHp
                    },
                    new HpThresholdConfig
                    {
                        HpPercentage = 0.60f,
                        SkillConditions = new List<SkillMonsterCondition>
                        {
                            new SkillMonsterCondition { SkillVNum = SkillsVnums.CARNO_JUMP, MonsterVNum = MonsterVnum.TWISTED_BEAST_KING_CARNO }
                        },
                        Trigger = BattleTriggers.OnSixtyPercentHp
                    }
                }
            },
            new MonsterHpThresholdConfig
            {
                MonsterVNum = (int)MonsterVnum.LAURENA,
                HpThresholds = new List<HpThresholdConfig>
                {
                    new HpThresholdConfig
                    {
                        HpPercentage = 0.75f,
                        SkillConditions = new List<SkillMonsterCondition>
                        {
                            new SkillMonsterCondition { SkillVNum = SkillsVnums.LAURENA_BUFF_TELEPORT, MonsterVNum = MonsterVnum.LAURENA }
                        },
                        Trigger = BattleTriggers.OnThreeFourthsHp
                    },
                    new HpThresholdConfig
                    {
                        HpPercentage = 0.50f,
                        SkillConditions = new List<SkillMonsterCondition>
                        {
                            new SkillMonsterCondition { SkillVNum = SkillsVnums.LAURENA_BUFF_TELEPORT, MonsterVNum = MonsterVnum.LAURENA }
                        },
                        Trigger = BattleTriggers.OnHalfHp
                    },
                    new HpThresholdConfig
                    {
                        HpPercentage = 0.25f,
                        SkillConditions = new List<SkillMonsterCondition>
                        {
                            new SkillMonsterCondition { SkillVNum = SkillsVnums.LAURENA_BUFF_TELEPORT, MonsterVNum = MonsterVnum.LAURENA }
                        },
                        Trigger = BattleTriggers.OnQuarterHp
                    }
                }
            }
        };
        if (defender is IMonsterEntity monsterEntity && !configs.Any(c => c.MonsterVNum == monsterEntity.MonsterVNum))
        {
            configs.Add(new MonsterHpThresholdConfig
            {
                MonsterVNum = monsterEntity.MonsterVNum,
                HpThresholds = new List<HpThresholdConfig>
                {
                    new HpThresholdConfig { HpPercentage = 0.90f, Trigger = BattleTriggers.OnNinetyPercentHp },
                    new HpThresholdConfig { HpPercentage = 0.75f, Trigger = BattleTriggers.OnThreeFourthsHp },
                    new HpThresholdConfig { HpPercentage = 0.60f, Trigger = BattleTriggers.OnSixtyPercentHp },
                    new HpThresholdConfig { HpPercentage = 0.50f, Trigger = BattleTriggers.OnHalfHp },
                    new HpThresholdConfig { HpPercentage = 0.25f, Trigger = BattleTriggers.OnQuarterHp }
                }
            });
        }
        var hpThresholdManager = new HpThresholdManager(configs, _asyncEventPipeline);
        await hpThresholdManager.ProcessHpThresholdsAsync(defender, initialDefenderHp, cancellation);
        
        switch (attacker)
        {
            case IPlayerEntity playerEntity:
                playerEntity.LastAttack = DateTime.UtcNow;
                break;
        }

        switch (defender)
        {
            case IPlayerEntity character:
                {
                    character.LastDefence = DateTime.UtcNow;

                    character.Session.RefreshStat();

                    if (character.IsSitting)
                    {
                        await character.Session.RestAsync(force: true);
                    }

                    break;
                }
            case IMateEntity mate:
                {
                    mate.LastDefence = DateTime.UtcNow;

                    mate.Owner.Session.SendMateLife(mate);

                    if (mate.IsSitting)
                    {
                        await mate.Owner.Session.EmitEventAsync(new MateRestEvent
                        {
                            MateEntity = mate,
                            Force = true
                        });
                    }

                    break;
                }
            case IMonsterEntity { Hp: <= 0, IsStillAlive: false }:
                return;
        }

        if (!defender.IsAlive())
        {
            await defender.EmitEventAsync(new GenerateEntityDeathEvent
            {
                Entity = defender,
                Attacker = attacker,
                IsByMainWeapon = !skillInfo.IsUsingSecondWeapon
            });
        }
    }

    private async Task MateUpgradeProgress(IBattleEntity attacker, IBattleEntity defender)
    {
        if (attacker.IsMate())
        {
            if (!defender.IsMonster())
            {
                return;
            }

            var monster = (IMonsterEntity)defender;
            if (!monster.IsMateTrainer)
            {
                return;
            }

            var mate = (IMateEntity)attacker;

            if (mate.MateType == MateType.Partner)
            {
                return;
            }

            await mate.Owner.Session.EmitEventAsync(new MateCheckUpgradeProgressEvent(mate, monster, true));
            return;
        }

        if (!defender.IsMate())
        {
            return;
        }

        if (!attacker.IsMonster())
        {
            return;
        }

        var attackerMonster = (IMonsterEntity)attacker;
        if (!attackerMonster.IsMateTrainer)
        {
            return;
        }

        var defenderMate = (IMateEntity)defender;
        if (defenderMate.MateType == MateType.Partner)
        {
            return;
        }

        defenderMate.Owner.Session.EmitEventAsync(new MateCheckUpgradeProgressEvent(defenderMate, attackerMonster, false)).ConfigureAwait(false).GetAwaiter().GetResult();
    }
    
    private async Task TrainerSpecialistMateLevelUp(IBattleEntity attacker, IBattleEntity defender)
    {
        if (defender.IsMate() && !attacker.IsPlayer())
        {
            if (attacker is IMonsterEntity monster && monster.IsSparringMonster())
            {
                var mate = (IMateEntity)defender;

                if (mate.MateType == MateType.Partner)
                {
                    return;
                }

                await mate.Owner.Session.EmitEventAsync(new TrainerSpecialistMateLevelUpEvent(mate, monster));
            }
        }
    }

    private void TryLoseLoyalty(IBattleEntity attacker, IBattleEntity defender)
    {
        if (defender is not IMateEntity mateEntity)
        {
            return;
        }

        if (attacker is IMonsterEntity { IsMateTrainer: true, IsSparringMonster: true })
        {
            return;
        }

        if (_randomGenerator.RandomNumber() > 2)
        {
            return;
        }

        mateEntity.RemoveLoyalty((short)_randomGenerator.RandomNumber(1, 6), _minMaxConfiguration, _gameLanguage);
    }

    private int GenerateNamajuDamage(IMonsterEntity namaju)
    {
        BCardDTO bCard = namaju.BCards.FirstOrDefault(x =>
            x.Type == (short)BCardType.RecoveryAndDamagePercent && x.SubType == (byte)AdditionalTypes.RecoveryAndDamagePercent.DecreaseSelfHP);
        if (bCard == null)
        {
            return default;
        }

        return (int)(namaju.MaxHp * (bCard.FirstData * 0.01));
    }

    private async Task RemovePvPHardcodedBuff(IBattleEntity attacker, IBattleEntity defender)
    {
        if (attacker is not IPlayerEntity playerAttacker)
        {
            return;
        }

        if (defender is not IPlayerEntity playerDefender)
        {
            return;
        }

        if (attacker.IsSameEntity(defender))
        {
            return;
        }

        DateTime now = DateTime.UtcNow;
        playerAttacker.LastPvPAttack = now;
        playerDefender.LastPvPAttack = now;

        short[] pvpBuffs = _buffsToRemoveConfig.GetBuffsToRemove(BuffsToRemoveType.PVP);

        if (playerAttacker.BuffComponent.HasAnyBuff())
        {
            foreach (short buff in pvpBuffs)
            {
                foreach (IMateEntity teamMember in playerAttacker.MateComponent.TeamMembers())
                {
                    Buff toRemoveMate = teamMember.BuffComponent.GetBuff(buff);
                    await teamMember.RemoveBuffAsync(false, toRemoveMate);
                }

                Buff toRemove = playerAttacker.BuffComponent.GetBuff(buff);
                await playerAttacker.RemoveBuffAsync(false, toRemove);
            }

            foreach (Buff buff in playerAttacker.BuffComponent.GetAllBuffs(x => x.IsDisappearOnPvp()))
            {
                await playerAttacker.RemoveBuffAsync(false, buff);
            }
        }

        if (!playerDefender.BuffComponent.HasAnyBuff())
        {
            return;
        }

        foreach (short buff in pvpBuffs)
        {
            foreach (IMateEntity teamMember in playerDefender.MateComponent.TeamMembers())
            {
                Buff toRemoveMate = teamMember.BuffComponent.GetBuff(buff);
                await teamMember.RemoveBuffAsync(false, toRemoveMate);
            }

            Buff toRemove = playerDefender.BuffComponent.GetBuff(buff);
            await playerDefender.RemoveBuffAsync(false, toRemove);
        }

        foreach (Buff buff in playerDefender.BuffComponent.GetAllBuffs(x => x.IsDisappearOnPvp()))
        {
            await playerDefender.RemoveBuffAsync(false, buff);
        }
    }

    private async Task HealDefenderByGivenDamage(IBattleEntity defender, int damage)
    {
        if (defender is not IPlayerEntity playerEntity)
        {
            return;
        }

        int toHeal = playerEntity.GetMaxArmorShellValue(ShellEffectType.RecoveryHPInDefence);

        if (toHeal == 0)
        {
            return;
        }

        if (!playerEntity.IsAlive())
        {
            return;
        }

        if (toHeal > 0)
        {
            int heal = (int)(damage * (toHeal * 0.01 / 5));
            await playerEntity.EmitEventAsync(new BattleEntityHealEvent
            {
                Entity = playerEntity,
                HpHeal = heal
            });
        }
    }

    private void AddMonsterHitsToPlayer(IBattleEntity attacker, IBattleEntity defender)
    {
        if (attacker is not IMonsterEntity monsterEntity)
        {
            return;
        }

        IPlayerEntity playerEntity = defender switch
        {
            IMateEntity mateEntity => mateEntity.Owner,
            IPlayerEntity player => player,
            _ => null
        };

        if (playerEntity == null)
        {
            return;
        }

        if (!monsterEntity.DropToInventory && !monsterEntity.MapInstance.HasMapFlag(MapFlags.HAS_DROP_DIRECTLY_IN_INVENTORY_ENABLED) && 
            !monsterEntity.MapInstance.HasMapFlag(MapFlags.ACT_4) && !playerEntity.HasAutoLootEnabled && monsterEntity.MapInstance.MapInstanceType != MapInstanceType.PrivateInstance)
        {
            return;
        }

        if (!playerEntity.HitsByMonsters.TryGetValue(monsterEntity.Id, out int hits))
        {
            playerEntity.HitsByMonsters.TryAdd(monsterEntity.Id, 1);
            return;
        }

        hits++;
        playerEntity.HitsByMonsters[monsterEntity.Id] = hits;
    }

    private void AddPlayerDamageToMonster(IBattleEntity attacker, IBattleEntity defender, int damage)
    {
        if (defender is not IMonsterEntity monsterEntity)
        {
            return;
        }

        IPlayerEntity playerEntity = attacker switch
        {
            IMateEntity mateEntity => mateEntity.Owner,
            IMonsterEntity monster => monster.SummonerType is VisualType.Player && monster.SummonerId.HasValue ? attacker.MapInstance.GetCharacterById(monster.SummonerId.Value) : null,
            IPlayerEntity player => player,
            _ => null
        };

        if (playerEntity == null)
        {
            return;
        }

        if (!monsterEntity.DropToInventory && !monsterEntity.MapInstance.HasMapFlag(MapFlags.HAS_DROP_DIRECTLY_IN_INVENTORY_ENABLED) && 
            !monsterEntity.MapInstance.HasMapFlag(MapFlags.ACT_4) && !playerEntity.HasAutoLootEnabled && monsterEntity.MapInstance.MapInstanceType != MapInstanceType.PrivateInstance)
        {
            return;
        }

        if (!monsterEntity.PlayersDamage.TryGetValue(playerEntity.Id, out int playerDamage))
        {
            monsterEntity.PlayersDamage.TryAdd(playerEntity.Id, damage);
            return;
        }

        playerDamage += damage;
        monsterEntity.PlayersDamage[playerEntity.Id] = playerDamage;
    }
    
    private void IncreaseUltimateSkill(IBattleEntity attacker, SkillInfo skill, ref int damage)
    {
        int[] ultimateSkill = {(int)SkillsVnums.ULTIMATE_SONIC_WAVE, (int)SkillsVnums.ULTIMATE_TORNADO_KICK, (int)SkillsVnums.ULTIMATE_UPPERCUT, (int)SkillsVnums.ULTIMATE_TRI_COMBO };
        if (!ultimateSkill.Contains(skill.Vnum))
        {
            return;
        }
        (int firstData, _) = attacker.BCardComponent.GetAllBCardsInformation(BCardType.SummonAndRecoverHP, 
            (byte)AdditionalTypes.SummonAndRecoverHP.UltimateDamageIncreased, attacker.Level);
        if (firstData == 0)
        {
            return;
        }
        damage = (int)(damage * (firstData * 0.01));
    }
    
    private void IncreaseDamageByMarkedBuff(IBattleEntity defender, ref int damage)
     {
         Buff buff = defender.BuffComponent.GetBuff(692);
         if (buff == null || buff.IsFirstApply)
         {
             if (buff != null)
             {
                 buff.IsFirstApply = false;
             }

             return;
         }
    
         (int firstData, _) = defender.BCardComponent.GetAllBCardsInformation(BCardType.MysticArts,
             (byte)AdditionalTypes.MysticArts.SignUseNextAttackIncrease, defender.Level);
         if (firstData == 0)
         {
             return;
         }
    
         damage = (int)(damage * (firstData * 0.01));
         defender.RemoveBuffAsync(692).ConfigureAwait(false).GetAwaiter().GetResult();
     }
    
     private void IncreaseDamageByMarkedBuff2(IBattleEntity defender, ref int damage)
     {
         Buff buff = defender.BuffComponent.GetBuff(691);
         if (buff == null || buff.IsFirstApply)
         {
             if (buff != null)
             {
                 buff.IsFirstApply = false;
             }

             return;
         }
    
         (int firstData, _) = defender.BCardComponent.GetAllBCardsInformation(BCardType.MysticArts,
             (byte)AdditionalTypes.MysticArts.SignUseNextAttackIncrease, defender.Level);
         if (firstData == 0)
         {
             return;
         }
    
         damage = (int)(damage * (firstData * 0.01));
         defender.RemoveBuffAsync(691).ConfigureAwait(false).GetAwaiter().GetResult();
     }

    private async Task OpportunityToAttack(IBattleEntity defender)
    {
        if (!defender.IsAlive())
        {
            return;
        }

        if (!defender.BCardComponent.HasBCard(BCardType.MysticArtsTransformed, (byte)AdditionalTypes.MysticArtsTransformed.AchieveWhenAttacked))
        {
            return;
        }

        (int firstData, int secondData) =
         defender.BCardComponent.GetAllBCardsInformation(BCardType.MysticArtsTransformed,
         (byte)AdditionalTypes.MysticArtsTransformed.AchieveWhenAttacked, defender.Level);

        if (_randomGenerator.RandomNumber() > firstData)
        {
            return;
        }

        Buff debuff = _buffFactory.CreateBuff(secondData, defender);
        defender.BroadcastEffectInRange(EffectType.Withstand);
        defender.BroadcastEffectInRange(EffectType.PetAttack);
        await defender.AddBuffAsync(debuff);
        await defender.RemoveBuffAsync((int)BuffVnums.WITHSTAND);
    }

    private async Task HpLostWhileCasting(IBattleEntity attacker)
    {
        if (!attacker.IsAlive())
        {
            return;
        }

        if (!attacker.BCardComponent.HasBCard(BCardType.HealingBurningAndCasting, (byte)AdditionalTypes.HealingBurningAndCasting.DecreaseHPWhenCasting))
        {
            return;
        }

        int firstData = attacker.BCardComponent.GetAllBCardsInformation(
            BCardType.HealingBurningAndCasting,
            (byte)AdditionalTypes.HealingBurningAndCasting.DecreaseHPWhenCasting,
            attacker.Level).firstData;
        
        if (attacker.Hp - firstData <= 0)
        {
            if (attacker.Hp != 1)
            {
                attacker.BroadcastDamage(attacker.Hp - 1);
            }

            attacker.Hp = 1;
        }
        else
        {
            attacker.BroadcastDamage(firstData);
            attacker.Hp -= firstData;
        }
    }
    
    private void ReduceAbsorptionShellDmg(IBattleEntity defender, ref int damage)
    {
        if (!defender.IsAlive())
        {
            return;
        }

        if (defender is not IPlayerEntity player)
        {
            return;
        }
        int toHeal = player.GetMaxArmorShellValue(ShellEffectType.AbsorbDamagePercentageA);
        toHeal += player.GetMaxArmorShellValue(ShellEffectType.AbsorbDamagePercentageB);
        toHeal += player.GetMaxArmorShellValue(ShellEffectType.AbsorbDamagePercentageC);
        if (toHeal == 0)
        {
            return;
        }
        int hpToIncrease = (int)(damage * (toHeal * 0.01));
        defender.EmitEvent(new BattleEntityHealEvent
        {
            Entity = defender,
            HpHeal = hpToIncrease
        });
        damage -= toHeal;
    }
    
    private async Task ApplyBuffWhileBlock(IBattleEntity defender)
    {
        if (!defender.IsAlive())
        {
            return;
        }

        if (defender is not IPlayerEntity player)
        {
            return;
        }

        if (!defender.BCardComponent.HasBCard(BCardType.DefencePiercing, (byte)AdditionalTypes.DefencePiercing.ReceiveWhenBlockingWithBuffActive))
        {
            return;
        }
        
        (int firstData, int secondData) = 
            defender.BCardComponent.GetAllBCardsInformation(BCardType.DefencePiercing, 
                (byte)AdditionalTypes.DefencePiercing.ReceiveWhenBlockingWithBuffActive, defender.Level);

        if (!defender.BuffComponent.HasBuff(secondData))
        {
            return;
        }
        
        await defender.AddBuffAsync(_buffFactory.CreateBuff(firstData, defender));
    }

    private async Task ApplyDebuffEffectWhileShadowSkillElemTriggered(IBattleEntity attacker, IBattleEntity defender, SkillInfo skill)
    {
        if (skill.Element != (short)ElementType.Shadow)
        {
            return;
        }

        if (!attacker.IsAlive() || !defender.IsAlive())
        {
            return;
        }

        if (!defender.BCardComponent.HasBCard(BCardType.IncreaseDamageDebuffs, (byte)AdditionalTypes.IncreaseDamageDebuffs.TriggerOnShadowElementAttack))
        {
            return;
        }

        (int firstData, int secondData) = 
            defender.BCardComponent.GetAllBCardsInformation(BCardType.IncreaseDamageDebuffs, 
            (byte)AdditionalTypes.IncreaseDamageDebuffs.TriggerOnShadowElementAttack, defender.Level);

        if (_randomGenerator.RandomNumber() > firstData)
        {
            return;
        }

        Buff debuff = _buffFactory.CreateBuff(secondData, attacker);
        await defender.AddBuffAsync(debuff);
    }

    private void ReduceDamageByMp(IBattleEntity defender, ref int damage)
    {
        (int firstData, _) = defender.BCardComponent.GetAllBCardsInformation(BCardType.LightAndShadow, (byte)AdditionalTypes.LightAndShadow.InflictDamageToMP, defender.Level);
        if (firstData == 0)
        {
            return;
        }

        (int firstDataPositive, int _) = defender.BCardComponent.GetAllBCardsInformation(BCardType.HealingBurningAndCasting,
            (byte)AdditionalTypes.HealingBurningAndCasting.HPIncreasedByConsumingMP, defender.Level);

        int defenderMp = defender.Mp;
        int defenderMpToRemove = defender.CalculateManaUsage((int)(damage * (firstData * 0.01)));

        if (defenderMp - defenderMpToRemove <= 0)
        {
            damage -= defenderMp;
            defender.Mp = 0;

            int hpToAdd = (int)(firstDataPositive / 100.0 * defenderMp);
            defender.EmitEvent(new BattleEntityHealEvent
            {
                Entity = defender,
                HpHeal = hpToAdd
            });
        }
        else
        {
            damage -= defenderMpToRemove;
            defender.Mp -= defenderMpToRemove;

            int hpToAdd = (int)(firstDataPositive / 100.0 * defenderMpToRemove);
            defender.EmitEvent(new BattleEntityHealEvent
            {
                Entity = defender,
                HpHeal = hpToAdd
            });
        }
    }
    
    private void MpIncreasedByDmg(IBattleEntity attacker, IBattleEntity defender, int damage)
    {
        if (!attacker.IsAlive() || !defender.IsAlive())
        {
            return;
        }

        if (!attacker.BCardComponent.HasBCard(BCardType.Reflection, (byte)AdditionalTypes.Reflection.EnemyMPIncreased))
        {
            return;
        }
        
        if (attacker.BCardComponent.HasBCard(BCardType.FourthGlacernonFamilyRaid, (byte)AdditionalTypes.FourthGlacernonFamilyRaid.DisableHPMPRecovery))
        {
            return;
        }

        int firstData = attacker.BCardComponent.GetAllBCardsInformation(BCardType.Reflection, (byte)AdditionalTypes.Reflection.EnemyMPIncreased, attacker.Level).firstData;
        int mpToIncrease = (int)(damage * (firstData * 0.01));
        if (defender.Mp + mpToIncrease < defender.MaxMp)
        {
            defender.Mp += mpToIncrease;
        }
        else
        {
            defender.Mp = defender.MaxMp;
        }
    }
    
    private void MpDecreasedByDmg(IBattleEntity attacker, IBattleEntity defender, int damage)
    {
        if (!attacker.IsAlive() || !defender.IsAlive())
        {
            return;
        }

        if (!attacker.BCardComponent.HasBCard(BCardType.Reflection, (byte)AdditionalTypes.Reflection.EnemyMPDecreased))
        {
            return;
        }

        int firstData = attacker.BCardComponent.GetAllBCardsInformation(BCardType.Reflection, (byte)AdditionalTypes.Reflection.EnemyMPDecreased, attacker.Level).firstData;
        int mpToDecrease = (int)(damage * (firstData * 0.01));
        if (defender.Mp - mpToDecrease < defender.MaxMp)
        {
            defender.Mp -= mpToDecrease;
        }
        else
        {
            defender.Mp = defender.MaxMp;
        }
    }
    
    private async Task DecreaseHealByGivenDamage(IBattleEntity attacker, int damage)
    {
        if (!attacker.IsAlive())
        {
            return;
        }

        if (!attacker.BCardComponent.HasBCard(BCardType.Reflection, (byte)AdditionalTypes.Reflection.HPDecreased))
        {
            return;
        }

        int firstData = attacker.BCardComponent.GetAllBCardsInformation(BCardType.Reflection, (byte)AdditionalTypes.Reflection.HPDecreased, attacker.Level).firstData;
        int hpToDecrease = (int)(damage * (firstData * 0.01));
        await attacker.EmitEventAsync(new BattleEntityHealEvent
        {
            Entity = attacker,
            HpHeal = -hpToDecrease
        });
        attacker.BroadcastDamage(hpToDecrease);
    }

    private void ProcessBuffDamage(IBattleEntity defender, int damage)
    {
        if (!defender.BuffComponent.HasAnyBuff())
        {
            return;
        }

        if (defender.EndBuffDamages.Count == 0)
        {
            return;
        }

        var listToRemove = new ConcurrentQueue<short>();

        foreach (short buffVnum in defender.EndBuffDamages.Keys)
        {
            if (!defender.BuffComponent.HasBuff(buffVnum))
            {
                defender.RemoveEndBuffDamage(buffVnum);
                continue;
            }

            int damageAfter = defender.DecreaseDamageEndBuff(buffVnum, damage);
            if (damageAfter > 0)
            {
                continue;
            }

            Buff buffToRemove = defender.BuffComponent.GetBuff(buffVnum);
            defender.RemoveBuffAsync(false, buffToRemove).ConfigureAwait(false).GetAwaiter().GetResult();
            listToRemove.Enqueue(buffVnum);
        }

        while (listToRemove.TryDequeue(out short toRemoveBuff))
        {
            defender.RemoveEndBuffDamage(toRemoveBuff);
        }
    }

    private async Task RemoveDamagerHardcodedBuff(IBattleEntity damager)
    {
        var listToRemove = new List<Buff>();
        foreach (short buffVnum in _buffsToRemoveConfig.GetBuffsToRemove(BuffsToRemoveType.ATTACKER))
        {
            if (!damager.BuffComponent.HasBuff(buffVnum))
            {
                continue;
            }

            listToRemove.Add(damager.BuffComponent.GetBuff(buffVnum));
        }

        await damager.EmitEventAsync(new BuffRemoveEvent
        {
            Entity = damager,
            Buffs = listToRemove.AsReadOnly(),
            RemovePermanentBuff = false
        });
    }

    private async Task RemoveDamagedHardcodedBuff(IBattleEntity damaged, SkillInfo skillInfo)
    {
        if (damaged.BuffComponent.HasBuff((short)BuffVnums.MAGICAL_FETTERS) && damaged.IsPlayer())
        {
            var characterMagical = (IPlayerEntity)damaged;
            await characterMagical.Session.EmitEventAsync(new AngelSpecialistElementalBuffEvent
            {
                Skill = skillInfo
            });
        }

        var listToRemove = new List<Buff>();
        foreach (short buffVnum in _buffsToRemoveConfig.GetBuffsToRemove(BuffsToRemoveType.DEFENDER))
        {
            if (!damaged.BuffComponent.HasBuff(buffVnum))
            {
                continue;
            }

            listToRemove.Add(damaged.BuffComponent.GetBuff(buffVnum));
        }

        await damaged.EmitEventAsync(new BuffRemoveEvent
        {
            Entity = damaged,
            Buffs = listToRemove.AsReadOnly(),
            RemovePermanentBuff = false
        });

        if (damaged is IPlayerEntity character)
        {
            if (!_meditationManager.HasMeditation(character))
            {
                return;
            }

            _meditationManager.RemoveAllMeditation(character);
            foreach (BuffVnums buffVnum in _meditationBuffs)
            {
                Buff buff = character.BuffComponent.GetBuff((short)buffVnum);
                await damaged.RemoveBuffAsync(false, buff);
            }

            await damaged.AddBuffAsync(_buff.CreateBuff((short)BuffVnums.KUNDALINI_SYNDROME, damaged));
        }
    }

    private void VoodooIncreaseDamagePerDebuff(IBattleEntity defender, SkillInfo skillInfo, ref int damage)
    {
        if (skillInfo?.BCards == null || !skillInfo.BCards.Any(x => x.Type == (short)BCardType.VoodooPriest && x.SubType ==
                (byte)AdditionalTypes.VoodooPriest.IncreaseFixedDamageDebuffStack))
        {
            return;
        }

        BCardDTO bCardSkill = skillInfo.BCards.FirstOrDefault(x => x.Type == (short)BCardType.VoodooPriest && x.SubType ==
            (byte)AdditionalTypes.VoodooPriest.IncreaseFixedDamageDebuffStack);

        BCardDTO bCardInformation = skillInfo.BCards.FirstOrDefault(x => x.Type == (short)BCardType.HideBarrelSkill && x.SubType ==
            (byte)AdditionalTypes.HideBarrelSkill.AppliesWithDebuffAboveLevel);
    
        if (bCardInformation == null || defender?.BuffComponent == null)
        {
            return;
        }
    
        IReadOnlyList<Buff> debuffs = defender.BuffComponent.GetAllBuffs(x => x.BuffGroup == BuffGroup.Bad && x.Level > bCardInformation.FirstData);

        if (bCardSkill == null)
        {
            return;
        }

        damage += bCardSkill.FirstData * debuffs.Count;
        damage = Math.Min(damage, bCardSkill.SecondData);
    }

    private void StoreVoodooDamage(IBattleEntity damager, SkillInfo skillInfo, int damage)
    {
        if (damager is not IPlayerEntity player)
        {
            return;
        }

        if (!skillInfo.BCards.Any(x => 
                x.Type == (short)BCardType.FireCannoneerRangeBuff && 
                x.SubType == (byte)AdditionalTypes.FireCannoneerRangeBuff.StoreDamageCausedBySkill))
        {
            return;
        }

        BCardDTO? bCardSkill = skillInfo.BCards.FirstOrDefault(x =>
            x.Type == (short)BCardType.FireCannoneerRangeBuff && 
            x.SubType == (byte)AdditionalTypes.FireCannoneerRangeBuff.StoreDamageCausedBySkill);

        if (bCardSkill == null)
        {
            return;
        }

        int stored = (int)(damage * (bCardSkill.FirstData * 0.01)); 
        player.BCardDataComponent.VoodooDamageStored = Math.Min(
            (player.BCardDataComponent.VoodooDamageStored ?? 0) + stored, 
            bCardSkill.SecondData
        );
    }

    private void ExecuteVoodooDamageStored(IBattleEntity damager, SkillInfo skillInfo, ref int damage)
    {
        if (damager is not IPlayerEntity player)
        {
            return;
        }

        if (!player.BCardComponent.HasBCard(BCardType.BearSpirit, (byte)AdditionalTypes.BearSpirit.DamageNextSkillIncreased))
        {
            return;
        }

        if (skillInfo.CastId is 0 or 9)
        {
            return;
        }

        if (player.BCardDataComponent.VoodooDamageStored.HasValue)
        {
            damage += player.BCardDataComponent.VoodooDamageStored.Value;
            player.BCardDataComponent.VoodooDamageStored = null;
        }
        
        IReadOnlyList<Buff> buffToRemove = player.BuffComponent.GetAllBuffs(x =>
            x.BCards.Any(t => t.Type == (short)BCardType.BearSpirit &&
                t.SubType == (byte)AdditionalTypes.BearSpirit.DamageNextSkillIncreased));

        foreach (Buff buff in buffToRemove)
        {
            player.RemoveBuffAsync(false, buff).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }

}