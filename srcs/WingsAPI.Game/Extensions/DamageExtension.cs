using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Data.Families;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Bonus;
using WingsEmu.DTOs.Maps;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.EntityStatistics;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Helpers.Damages.Calculation;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Monster;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Game.Extensions;

public static class DamageExtension
{
    private static readonly double[] Plus = [0, 0.1, 0.15, 0.22, 0.32, 0.43, 0.54, 0.65, 0.9, 1.2, 2];
    private static IRandomGenerator RandomGenerator => StaticRandomGenerator.Instance;

    public static bool IsMiss(this IBattleEntityDump attacker, IBattleEntityDump defender, CalculationBasicStatistics basicStatistics, SkillInfo skill)
    {
        bool isPvP = attacker.IsPlayer() && defender.IsPlayer();
        int morale = basicStatistics.AttackerMorale;
        int attackerHitRate = basicStatistics.AttackerHitRate;
        int targetMorale = basicStatistics.DefenderMorale;
        int targetDodge = basicStatistics.DefenderDodge;
        
        if (attacker.IsMonster())
        {
            if (attacker is NpcMonsterEntityDump { MonsterRaceType: MonsterRaceType.Fixed, MonsterRaceSubType: MonsterSubRace.Fixed.EnergyBall })
            {
                return false;
            }
        }
        
        if (attacker.HasBCard(BCardType.Casting, (byte)AdditionalTypes.Casting.CastingSkillFailed))
        {
            return true;
        }

        if (attacker.HasBCard(BCardType.SpecialAttack, (byte)AdditionalTypes.SpecialAttack.FailIfMiss) && attacker.AttackType != AttackType.Magical)
        {
            return true;
        }
        
        if (defender.HasBCard(BCardType.SpecialDefence, (byte)AdditionalTypes.SpecialDefence.NoDefence))
        {
            return false;
        }

        if (defender.HasBCard(BCardType.FourthGlacernonFamilyRaid, (byte)AdditionalTypes.FourthGlacernonFamilyRaid.DisableHPMPConsumption))
        {
            return true;
        }
        
        if (defender.HasBCard(BCardType.MysticArts, (byte)AdditionalTypes.MysticArts.DodgeAndMakeChance))
        {
            int firstData = defender.GetBCardInformation(BCardType.MysticArts, 
                (byte)AdditionalTypes.MysticArts.DodgeAndMakeChance).firstData;
            
            if (defender.IsSucceededChance(firstData))
            {
                defender.BroadcastEffect(EffectType.SideStep);
                return true;
            }
        }
        
        switch (isPvP)
        {
            case true when attacker.GetShellWeaponEffectValue(ShellEffectType.NeverMissInPVP) > 0:
                return false;
            case true:
            {
                int shellChance = attacker.AttackType switch
                {
                    AttackType.Melee => defender.GetShellArmorEffectValue(ShellEffectType.CloseDefenceDodgeInPVP),
                    AttackType.Ranged => defender.GetShellArmorEffectValue(ShellEffectType.DistanceDefenceDodgeInPVP),
                    AttackType.Magical => defender.GetShellArmorEffectValue(ShellEffectType.IgnoreMagicDamage),
                    _ => 0
                };

                shellChance += defender.GetShellArmorEffectValue(ShellEffectType.DodgeAllDamage);

                if (shellChance != 0 && RandomGenerator.RandomNumber() <= shellChance)
                {
                    return true;
                }

                break;
            }
        }

        if (attacker.AttackType == AttackType.Magical)
        {
            return false;
        }
        
        int? hitOnChanceBCard = skill.BCards.FirstOrDefault(s => s.Type == (short)BCardType.GuarantedDodgeRangedAttack && s.SubType == (byte)AdditionalTypes.GuarantedDodgeRangedAttack.AttackHitChance)?.FirstData;

        if (hitOnChanceBCard.HasValue && RandomGenerator.RandomNumber() >= hitOnChanceBCard)
        {
            return true;
        }

        int attackerHitRateFinal = attackerHitRate + morale * 4;
        double targetDodgeFinal = targetDodge + targetMorale * 4;
        double difference = attackerHitRateFinal - targetDodgeFinal;

        // formula by friends111
        double chance = 100 / Math.PI * Math.Atan(0.015 * difference + 2) + 50;
        if (chance <= 40)
        {
            chance = 40;
        }

        if (defender.HasBCard(BCardType.GuarantedDodgeRangedAttack, (byte)AdditionalTypes.GuarantedDodgeRangedAttack.AlwaysDodgeProbability))
        {
            chance = 100 - defender.GetBCardInformation(BCardType.GuarantedDodgeRangedAttack, (byte)AdditionalTypes.GuarantedDodgeRangedAttack.AlwaysDodgeProbability).firstData;
        }

        int hitChance = attacker.GetBCardInformation(BCardType.GuarantedDodgeRangedAttack, (byte)AdditionalTypes.GuarantedDodgeRangedAttack.AttackHitChance).firstData;

        if (hitChance != 0 && chance < hitChance)
        {
            chance = hitChance;
        }

        hitChance = attacker.GetBCardInformation(BCardType.GuarantedDodgeRangedAttack, (byte)AdditionalTypes.GuarantedDodgeRangedAttack.AttackHitChanceNegated).firstData;

        if (hitChance != 0 && chance > hitChance)
        {
            chance = hitChance;
        }

        return RandomGenerator.RandomNumber() >= chance;
    }

    public static CalculationBasicStatistics CalculateBasicStatistics(this IBattleEntityDump attacker, IBattleEntityDump defender, SkillInfo skill)
    {
        bool isPvP = attacker.IsPlayer() && defender.IsPlayer();
        bool isPvE = attacker.IsMonster() && defender.IsPlayer();
        
        #region Morale

        int attackerMorale = attacker.Morale;
        int defenderMorale = defender.Morale;

        if (!defender.HasBCard(BCardType.Morale, (byte)AdditionalTypes.Morale.LockMorale))
        {
            attackerMorale += attacker.GetBCardInformation(BCardType.Morale, (byte)AdditionalTypes.Morale.MoraleIncreased).firstData;
            attackerMorale -= attacker.GetBCardInformation(BCardType.Morale, (byte)AdditionalTypes.Morale.MoraleDecreased).firstData;
        }
        
        if (!attacker.HasBCard(BCardType.Morale, (byte)AdditionalTypes.Morale.IgnoreEnemyMorale))
        {
            defenderMorale += defender.GetBCardInformation(BCardType.Morale, (byte)AdditionalTypes.Morale.MoraleIncreased).firstData;
            defenderMorale -= defender.GetBCardInformation(BCardType.Morale, (byte)AdditionalTypes.Morale.MoraleDecreased).firstData;
        }

        if (attacker.HasBCard(BCardType.Morale, (byte)AdditionalTypes.Morale.MoraleHalved))
        {
            attackerMorale /= 2;
        }

        if (attacker.HasBCard(BCardType.Morale, (byte)AdditionalTypes.Morale.MoraleDoubled))
        {
            attackerMorale *= 2;
        }
        
        if (defender.HasBCard(BCardType.Morale, (byte)AdditionalTypes.Morale.MoraleHalved))
        {
            defenderMorale /= 2;
        }

        if (defender.HasBCard(BCardType.Morale, (byte)AdditionalTypes.Morale.MoraleDoubled))
        {
            defenderMorale *= 2;
        }

        #endregion
        
        #region Upgrades

        int attackerAttackUpgrade = attacker.AttackUpgrade;
        int defenderDefenseUpgrade = defender.DefenseUpgrade;
        
        attackerAttackUpgrade += attacker.GetBCardInformation(BCardType.AttackPower, (byte)AdditionalTypes.AttackPower.AttackLevelIncreased).firstData;
        attackerAttackUpgrade -= attacker.GetBCardInformation(BCardType.AttackPower, (byte)AdditionalTypes.AttackPower.AttackLevelDecreased).firstData;

        if (attacker.HasBCard(BCardType.CalculatingLevel, (byte)AdditionalTypes.CalculatingLevel.CalculatedAttackLevel) && attackerAttackUpgrade > 0)
        {
            attackerAttackUpgrade = 0;
        }
        
        defenderDefenseUpgrade += defender.GetBCardInformation(BCardType.Defence, (byte)AdditionalTypes.Defence.DefenceLevelIncreased).firstData;
        defenderDefenseUpgrade -= defender.GetBCardInformation(BCardType.Defence, (byte)AdditionalTypes.Defence.DefenceLevelDecreased).firstData;
        
        if (defender.HasBCard(BCardType.CalculatingLevel, (byte)AdditionalTypes.CalculatingLevel.CalculatedDefenceLevel) && defenderDefenseUpgrade > 0)
        {
            defenderDefenseUpgrade = 0;
        }
        
        if (defender.HasBCard(BCardType.DefencePiercing, (byte)AdditionalTypes.DefencePiercing.PiercesArmourThoroughlyIncreasesDamage) && defenderDefenseUpgrade > 0)
        {
            defenderDefenseUpgrade = 0;
        }

        #endregion
        
        #region Hit Rate

        int attackerHitRate = attacker.HitRate;

        attackerHitRate += attacker.GetBCardInformation(BCardType.Target, (byte)AdditionalTypes.Target.AllHitRateIncreased).firstData;
        attackerHitRate -= attacker.GetBCardInformation(BCardType.Target, (byte)AdditionalTypes.Target.AllHitRateDecreased).firstData;
        
        attackerHitRate += attacker.AttackType switch
        {
            AttackType.Melee => attacker.GetBCardInformation(BCardType.Target, (byte)AdditionalTypes.Target.MeleeHitRateIncreased).firstData,
            AttackType.Ranged => attacker.GetBCardInformation(BCardType.Target, (byte)AdditionalTypes.Target.RangedHitRateIncreased).firstData,
            _ => 0
        };

        attackerHitRate -= attacker.AttackType switch
        {
            AttackType.Melee => attacker.GetBCardInformation(BCardType.Target, (byte)AdditionalTypes.Target.MeleeHitRateDecreased).firstData,
            AttackType.Ranged => attacker.GetBCardInformation(BCardType.Target, (byte)AdditionalTypes.Target.RangedHitRateDecreased).firstData,
            _ => 0
        };
        
        if (defender.IsMonster() && attacker.MapInstance.GetCharacterById(attacker.Id) is { SubClass: SubClassType.ArrowLord } arrowLord)
        {
            int hitRateIncrease = arrowLord.TierLevel switch
            {
                1 => 1,
                2 => 1,
                3 => 2,
                4 => 2,
                5 => 3,
                _ => 0
            };

            attackerHitRate += hitRateIncrease;
        }
        
        attackerHitRate += attacker.GetFamilyUpgradeValue(FamilyUpgradeType.INCREASE_HITRATE);
        attackerHitRate += (int)(attackerHitRate * attacker.GetFirstDataMultiplier(BCardType.IncreaseSpPoints, (byte)AdditionalTypes.IncreaseSpPoints.AccuracyIncrease));
        attackerHitRate -= (int)(attackerHitRate * attacker.GetFirstDataMultiplier(BCardType.IncreaseSpPoints, (byte)AdditionalTypes.IncreaseSpPoints.AccuracyDecrease));
        attackerHitRate += (int)(attackerHitRate * attacker.GetFirstDataMultiplier(BCardType.MagicArmour, (byte)AdditionalTypes.MagicArmour.AccuracyIncrease));

        #endregion
        
        #region Critical Chance

        int attackerCriticalChance = attacker.CriticalChance;
        int defenderCriticalResistance = defender.CriticalDefence;
        
        attackerCriticalChance += attacker.GetBCardInformation(BCardType.Critical, (byte)AdditionalTypes.Critical.InflictingIncreased).firstData;
        attackerCriticalChance -= attacker.GetBCardInformation(BCardType.Critical, (byte)AdditionalTypes.Critical.InflictingReduced).firstData;
        
        attackerCriticalChance += attacker.GetShellWeaponEffectValue(ShellEffectType.CriticalChance);
        attackerCriticalChance -= defender.GetShellArmorEffectValue(ShellEffectType.ReducedCritChanceRecive);
        attackerCriticalChance += defender.GetFamilyUpgradeValue(FamilyUpgradeType.INCREASE_BOOST_CRITIC_POWER_ATTACK);
        
        defenderCriticalResistance -= defender.GetBCardInformation(BCardType.Critical, (byte)AdditionalTypes.Critical.ReceivingIncreased).firstData;
        defenderCriticalResistance += defender.GetBCardInformation(BCardType.Critical, (byte)AdditionalTypes.Critical.ReceivingDecreased).firstData;

        (int firstData, int secondData, int _) criticalIncreasedSkill = attacker.GetBCardInformation(BCardType.AbsorptionAndPowerSkill, (byte)AdditionalTypes.AbsorptionAndPowerSkill.CriticalIncreasedSkill);

        if (skill != null && skill.Vnum == criticalIncreasedSkill.secondData)
        {
            attackerCriticalChance += criticalIncreasedSkill.firstData;
        }
        
        (int firstData, int secondData, int _) criticalDecreasedSkill = attacker.GetBCardInformation(BCardType.AbsorptionAndPowerSkill, (byte)AdditionalTypes.AbsorptionAndPowerSkill.CriticalDecreasedSkill);

        if (skill != null && skill.Vnum == criticalDecreasedSkill.secondData)
        {
            attackerCriticalChance -= criticalDecreasedSkill.firstData;
        }
        
        criticalIncreasedSkill = defender.GetBCardInformation(BCardType.LordHatus, (byte)AdditionalTypes.LordHatus.InflictCriticalChanceWithSkill);

        if (skill != null && skill.Vnum == criticalIncreasedSkill.secondData && attacker.IsSucceededChance(criticalIncreasedSkill.firstData))
        {
            attackerCriticalChance = 100;
        }
        
        if (attacker.HasBCard(BCardType.DefencePiercing, (byte)AdditionalTypes.DefencePiercing.IncreaseCriticalHitChanceIfBuffActive))
        {
            (int firstData, int secondData, _) = attacker.GetBCardInformation(BCardType.DefencePiercing, (byte)AdditionalTypes.DefencePiercing.IncreaseCriticalHitChanceIfBuffActive);
            
            if (attacker.HasBuff(secondData))
            {
                attackerCriticalChance += (int)(attackerCriticalChance * firstData * 0.01);
            }
        }
        
        if (attacker.HasBCard(BCardType.DefencePiercing, (byte)AdditionalTypes.DefencePiercing.ReduceCriticalHitChanceIfBuffActive))
        {
            (int firstData, int secondData, _) = attacker.GetBCardInformation(BCardType.DefencePiercing, (byte)AdditionalTypes.DefencePiercing.ReduceCriticalHitChanceIfBuffActive);
            
            if (attacker.HasBuff(secondData))
            {
                attackerCriticalChance -= (int)(attackerCriticalChance * firstData * 0.01);
            }
        }
        
        if (isPvP && attacker.MapInstance.GetCharacterById(attacker.Id) is { SubClass: SubClassType.SilentStalker } silentStalker)
        {
            int criticalChanceIncrease = silentStalker.TierLevel switch
            {
                1 => 1,
                2 => 1, 
                3 => 2, 
                4 => 2, 
                5 => 3, 
                _ => 0 
            };

            attackerCriticalChance += criticalChanceIncrease; 
        }

        if (defender.IsMonster())
        {
            MonsterRaceType monsterRaceType = defender.MonsterRaceType;
            Enum monsterRaceSubType = defender.MonsterRaceSubType;

            if (attacker.HasBCard(BCardType.SpecialisationBuffResistance, (byte)AdditionalTypes.SpecialisationBuffResistance.IncreaseCriticalAgainst))
            {
                (int firstData, int secondData, int count) raceBCard =
                    attacker.GetBCardInformation(BCardType.SpecialisationBuffResistance, (byte)AdditionalTypes.SpecialisationBuffResistance.IncreaseCriticalAgainst);

                int monsterRace = raceBCard.firstData;
                var bCardRaceType = (MonsterRaceType)Math.Floor(monsterRace / 10.0);
                Enum bCardRaceSubType = attacker.GetRaceSubType(bCardRaceType, (byte)(monsterRace % 10));

                if (monsterRaceType == bCardRaceType && bCardRaceSubType != null && Equals(monsterRaceSubType, bCardRaceSubType))
                {
                    attackerCriticalChance += raceBCard.secondData;
                }
            }
            
            if (attacker.HasBCard(BCardType.SpecialisationBuffResistance, (byte)AdditionalTypes.SpecialisationBuffResistance.ReduceCriticalAgainst))
            {
                (int firstData, int secondData, int count) raceBCard =
                    attacker.GetBCardInformation(BCardType.SpecialisationBuffResistance, (byte)AdditionalTypes.SpecialisationBuffResistance.ReduceCriticalAgainst);

                int monsterRace = raceBCard.firstData;
                var bCardRaceType = (MonsterRaceType)Math.Floor(monsterRace / 10.0);
                Enum bCardRaceSubType = attacker.GetRaceSubType(bCardRaceType, (byte)(monsterRace % 10));

                if (monsterRaceType == bCardRaceType && bCardRaceSubType != null && Equals(monsterRaceSubType, bCardRaceSubType))
                {
                    attackerCriticalChance -= raceBCard.secondData;
                }
            }
        }

        attackerCriticalChance -= (int)(attackerCriticalChance * defenderCriticalResistance * 0.01);
        
        if (defender.HasBCard(BCardType.SniperAttack, (byte)AdditionalTypes.SniperAttack.ReceiveCriticalFromSniper) && skill is { Vnum: (short)SkillsVnums.SNIPER })
        {
            attackerCriticalChance = defender.GetBCardInformation(BCardType.SniperAttack, (byte)AdditionalTypes.SniperAttack.ReceiveCriticalFromSniper).firstData;
        }
        
        if (defender.HasBCard(BCardType.SpecialCritical, (byte)AdditionalTypes.SpecialCritical.InflictingChancePercent))
        {
            attackerCriticalChance = defender.GetBCardInformation(BCardType.SpecialCritical, (byte)AdditionalTypes.SpecialCritical.InflictingChancePercent).firstData;
        }

        if (defender.HasBCard(BCardType.SpecialCritical, (byte)AdditionalTypes.SpecialCritical.ReceivingChancePercent))
        {
            attackerCriticalChance = defender.GetBCardInformation(BCardType.SpecialCritical, (byte)AdditionalTypes.SpecialCritical.ReceivingChancePercent).firstData;
        }

        if (attacker.HasBCard(BCardType.SpecialCritical, (byte)AdditionalTypes.SpecialCritical.AlwaysInflict))
        {
            attackerCriticalChance = 100;
        }

        if (defender.HasBCard(BCardType.SpecialCritical, (byte)AdditionalTypes.SpecialCritical.AlwaysReceives))
        {
            attackerCriticalChance = 100;
        }

        if (defender.HasBCard(BCardType.SpecialCritical, (byte)AdditionalTypes.SpecialCritical.NeverReceives))
        {
            attackerCriticalChance = 0;
        }

        if (attacker.HasBCard(BCardType.SpecialCritical, (byte)AdditionalTypes.SpecialCritical.NeverInflict))
        {
            attackerCriticalChance = 0;
        }

        #endregion
        
        #region Critical Damage

        int attackerCriticalDamage = attacker.CriticalDamage;

        attackerCriticalDamage += attacker.GetShellWeaponEffectValue(ShellEffectType.CriticalDamage);
        attackerCriticalDamage -= defender.GetJewelsEffectValue(CellonType.CriticalDamageDecrease);
        
        attackerCriticalDamage += defender.GetFamilyUpgradeValue(FamilyUpgradeType.INCREASE_BOOST_POWER_ATTACK);
        attackerCriticalDamage -= defender.GetFamilyUpgradeValue(FamilyUpgradeType.INCOMING_CRITICAL_DMG_REDUC);
        
        attackerCriticalDamage += defender.GetBCardInformation(BCardType.Critical, (byte)AdditionalTypes.Critical.DamageFromCriticalIncreased).firstData;
        attackerCriticalDamage -= defender.GetBCardInformation(BCardType.Critical, (byte)AdditionalTypes.Critical.DamageFromCriticalDecreased).firstData;
        
        attackerCriticalDamage += attacker.GetBCardInformation(BCardType.Critical, (byte)AdditionalTypes.Critical.DamageIncreased).firstData;
        attackerCriticalDamage -= attacker.GetBCardInformation(BCardType.Critical, (byte)AdditionalTypes.Critical.DamageIncreasedInflictingReduced).firstData;
        
        attackerCriticalDamage -= defender.ChanceBCard(BCardType.StealBuff, (byte)AdditionalTypes.StealBuff.ReduceCriticalReceivedChance);

        attackerCriticalDamage -= defender.CriticalDefence;
        
        if (isPvP && attacker.MapInstance.GetCharacterById(attacker.Id) is { SubClass: SubClassType.CrimsonFury } crimsonFury)
        {
            int criticalDamageIncrease = crimsonFury.TierLevel switch
            {
                1 => 3,
                2 => 4, 
                3 => 4,
                4 => 5,
                5 => 5,
                _ => 0
            };

            attackerCriticalDamage += criticalDamageIncrease;
        }
        
        if (attacker.HasBCard(BCardType.DefencePiercing, (byte)AdditionalTypes.DefencePiercing.IncreaseCriticalDamageIfBuffActive))
        {
            (int firstData, int secondData, _) = attacker.GetBCardInformation(BCardType.DefencePiercing, (byte)AdditionalTypes.DefencePiercing.IncreaseCriticalDamageIfBuffActive);
            
            if (attacker.HasBuff(secondData))
            {
                attackerCriticalDamage += (int)(attackerCriticalDamage * firstData * 0.01);
            }
        }
        
        if (attacker.HasBCard(BCardType.DefencePiercing, (byte)AdditionalTypes.DefencePiercing.ReduceCriticalDamageIfBuffActive))
        {
            (int firstData, int secondData, _) = attacker.GetBCardInformation(BCardType.DefencePiercing, (byte)AdditionalTypes.DefencePiercing.ReduceCriticalDamageIfBuffActive);
            
            if (attacker.HasBuff(secondData))
            {
                attackerCriticalDamage -= (int)(attackerCriticalDamage * firstData * 0.01);
            }
        }
        
        if (attacker.HasBCard(BCardType.ReflectDamage, (byte)AdditionalTypes.ReflectDamage.CriticalAttackIncrease))
        {
            IBattleEntity battleEntity = attacker.MapInstance.GetBattleEntity(attacker.Type, attacker.Id);
            (int firstData, int secondData) = battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.ReflectDamage, 
                (byte)AdditionalTypes.ReflectDamage.CriticalAttackIncrease, battleEntity.Level);
        
            if (battleEntity.BCardDataComponent.CriticalDamageIncreased <= secondData)
            {
                attackerCriticalDamage += firstData * secondData;
            }
        }
        
        if (defender.HasBCard(BCardType.Block, (byte)AdditionalTypes.Block.DecreaseFinalCriticalDamagePerHit))
        {
            IBattleEntity battleEntity = defender.MapInstance.GetBattleEntity(defender.Type, defender.Id);
            (int firstData, int secondData) = battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.Block, 
                (byte)AdditionalTypes.Block.DecreaseFinalCriticalDamagePerHit, battleEntity.Level);
        
            if (battleEntity.BCardDataComponent.CriticalDamageDecreased <= secondData)
            {
                attackerCriticalDamage -= firstData * secondData;
            }
        }

        #endregion
        
        #region Element Rate

        int attackerElementRate = attacker.ElementRate;

        attackerElementRate += attacker.ChanceBCard(BCardType.IncreaseElementFairy, (byte)AdditionalTypes.IncreaseElementFairy.FairyElementIncreaseWhileAttackingChance);
        attackerElementRate -= attacker.ChanceBCard(BCardType.IncreaseElementFairy, (byte)AdditionalTypes.IncreaseElementFairy.FairyElementDecreaseWhileAttackingChance);

        attackerElementRate += attacker.GetFamilyUpgradeValue(FamilyUpgradeType.FAIRY_ELEMENT_BOOST);
        
        #endregion
        
        #region Dodge
        
        int defenderDodge = attacker.AttackType switch
        {
            AttackType.Melee => defender.MeleeDodge,
            AttackType.Ranged => defender.RangeDodge,
            _ => 0
        };

        defenderDodge += defender.GetBCardInformation(BCardType.DodgeAndDefencePercent, (byte)AdditionalTypes.DodgeAndDefencePercent.DodgeIncreased).firstData;
        defenderDodge -= defender.GetBCardInformation(BCardType.DodgeAndDefencePercent, (byte)AdditionalTypes.DodgeAndDefencePercent.DodgeDecreased).firstData;
        
        defenderDodge += defender.GetFamilyUpgradeValue(FamilyUpgradeType.INCREASE_DODGE);
        
        defenderDodge += attacker.AttackType switch
        {
            AttackType.Melee => defender.GetBCardInformation(BCardType.DodgeAndDefencePercent, (byte)AdditionalTypes.DodgeAndDefencePercent.DodgingMeleeIncreased).firstData,
            AttackType.Ranged => defender.GetBCardInformation(BCardType.DodgeAndDefencePercent, (byte)AdditionalTypes.DodgeAndDefencePercent.DodgingRangedIncreased).firstData,
            _ => 0
        };
        
        defenderDodge -= attacker.AttackType switch
        {
            AttackType.Melee => defender.GetBCardInformation(BCardType.DodgeAndDefencePercent, (byte)AdditionalTypes.DodgeAndDefencePercent.DodgingMeleeDecreased).firstData,
            AttackType.Ranged => defender.GetBCardInformation(BCardType.DodgeAndDefencePercent, (byte)AdditionalTypes.DodgeAndDefencePercent.DodgingRangedDecreased).firstData,
            _ => 0
        };

        double defenderDodgeMultiplier = 1;
        

        defenderDodgeMultiplier += defender.GetFirstDataMultiplier(BCardType.IncreaseDamageInLoD, (byte)AdditionalTypes.IncreaseDamageInLoD.DodgeIncrease);
        defenderDodgeMultiplier -= defender.GetFirstDataMultiplier(BCardType.IncreaseDamageInLoD, (byte)AdditionalTypes.IncreaseDamageInLoD.DodgeDecrease);
        defenderDodgeMultiplier += defender.GetFirstDataMultiplier(BCardType.MagicArmour, (byte)AdditionalTypes.MagicArmour.DodgeIncrease);

        defenderDodge = (int)(defenderDodge * defenderDodgeMultiplier);
        
        #endregion
        
        #region Attacker Defense

        int attackerMagicalDefense = attacker.MagicalDefense;

        attackerMagicalDefense += attacker.GetBCardInformation(BCardType.Defence, (byte)AdditionalTypes.Defence.AllIncreased).firstData;
        attackerMagicalDefense -= attacker.GetBCardInformation(BCardType.Defence, (byte)AdditionalTypes.Defence.AllDecreased).firstData;
        attackerMagicalDefense += attacker.GetBCardInformation(BCardType.Defence, (byte)AdditionalTypes.Defence.MagicalIncreased).firstData;
        attackerMagicalDefense -= attacker.GetBCardInformation(BCardType.Defence, (byte)AdditionalTypes.Defence.MagicalDecreased).firstData;

        attackerMagicalDefense += attacker.GetShellArmorEffectValue(ShellEffectType.MagicDefence);

        if (attacker.HasBCard(BCardType.SpecialDefence, (byte)AdditionalTypes.SpecialDefence.MagicDefenceNullified))
        {
            attackerMagicalDefense = 0;
        }
        
        if (attacker.HasBCard(BCardType.SpecialDefence, (byte)AdditionalTypes.SpecialDefence.AllDefenceNullified))
        {
            attackerMagicalDefense = 0;
        }

        #endregion
        
        #region Defender Defense
        
        int defenderDefenseArmour = attacker.AttackType switch
        {
            AttackType.Melee => defender.MeleeDefense,
            AttackType.Ranged => defender.RangeDefense,
            AttackType.Magical => defender.MagicalDefense,
            _ => 0
        };
        
        int defenderDefense = defenderDefenseArmour;
        
        defenderDefense += defender.GetBCardInformation(BCardType.Defence, (byte)AdditionalTypes.Defence.AllIncreased).firstData;
        defenderDefense -= defender.GetBCardInformation(BCardType.Defence, (byte)AdditionalTypes.Defence.AllDecreased).firstData;
        defenderDefense += defender.GetFamilyUpgradeValue(FamilyUpgradeType.DEFENCE_BOOST);
        
        if (defender.HasBCard(BCardType.VoodooPriest, (byte)AdditionalTypes.VoodooPriest.IncreaseDamageDebuffStack))
        {
            IBattleEntity target = defender.MapInstance.GetBattleEntity(defender.Type, defender.Id);
            IReadOnlyList<Buff> debuffsOngoing = target.BuffComponent.GetAllBuffs(x => x.BuffGroup == BuffGroup.Bad && x.BuffCategory is BuffCategory.PoisonType or BuffCategory.DiseaseSeries);
            (int firstData, int secondData) = target.BCardComponent.GetAllBCardsInformation(BCardType.VoodooPriest,
                (byte)AdditionalTypes.VoodooPriest.IncreaseDamageDebuffStack, target.Level);
            int debuffCount = debuffsOngoing.Count;
            defenderDefense -= (int)(debuffCount * firstData * 0.01 * Math.Min(secondData, debuffCount) / 100.0);
        }
        
        defenderDefense += attacker.AttackType switch
        {
            AttackType.Melee => defender.GetBCardInformation(BCardType.Defence, (byte)AdditionalTypes.Defence.MeleeIncreased).firstData,
            AttackType.Ranged => defender.GetBCardInformation(BCardType.Defence, (byte)AdditionalTypes.Defence.RangedIncreased).firstData,
            AttackType.Magical => defender.GetBCardInformation(BCardType.Defence, (byte)AdditionalTypes.Defence.MagicalIncreased).firstData,
            _ => 0
        };
        
        defenderDefense -= attacker.AttackType switch
        {
            AttackType.Melee => defender.GetBCardInformation(BCardType.Defence, (byte)AdditionalTypes.Defence.MeleeDecreased).firstData,
            AttackType.Ranged => defender.GetBCardInformation(BCardType.Defence, (byte)AdditionalTypes.Defence.RangedDecreased).firstData,
            AttackType.Magical => defender.GetBCardInformation(BCardType.Defence, (byte)AdditionalTypes.Defence.MagicalDecreased).firstData,
            _ => 0
        };
        
        defenderDefense += attacker.AttackType switch
        {
            AttackType.Melee => defender.GetShellArmorEffectValue(ShellEffectType.CloseDefence),
            AttackType.Ranged => defender.GetShellArmorEffectValue(ShellEffectType.DistanceDefence),
            AttackType.Magical => defender.GetShellArmorEffectValue(ShellEffectType.MagicDefence),
            _ => 0
        };
        
        double badBuffStackDefence = 1 + defender.GetBCardInformation(BCardType.IncreaseDamageDebuffs, (byte)AdditionalTypes.IncreaseDamageDebuffs.IncreasePowerOnDebuff).firstData * defender.BadBuffCounter * 0.01;
        defenderDefense = (int)(defenderDefense * badBuffStackDefence);
        
        bool noDefense = attacker.AttackType switch
        {
            AttackType.Melee => defender.HasBCard(BCardType.SpecialDefence, (byte)AdditionalTypes.SpecialDefence.MeleeDefenceNullified),
            AttackType.Ranged => defender.HasBCard(BCardType.SpecialDefence, (byte)AdditionalTypes.SpecialDefence.RangedDefenceNullified),
            AttackType.Magical => defender.HasBCard(BCardType.SpecialDefence, (byte)AdditionalTypes.SpecialDefence.MagicDefenceNullified),
            _ => false
        };

        if (noDefense)
        {
            defenderDefense = 0;
        }

        if (defender.HasBCard(BCardType.SpecialDefence, (byte)AdditionalTypes.SpecialDefence.AllDefenceNullified))
        {
            defenderDefense = 0;
        }
        
        #endregion

        #region Resistance
        
        int defenderResistance = attacker.Element switch
        {
            ElementType.Fire => defender.FireResistance,
            ElementType.Water => defender.WaterResistance,
            ElementType.Light => defender.LightResistance,
            ElementType.Shadow => defender.ShadowResistance,
            _ => 0
        };
        
        defenderResistance -= attacker.GetBCardInformation(BCardType.EnemyElementResistance, (byte)AdditionalTypes.EnemyElementResistance.AllDecreased).firstData;
        defenderResistance -= attacker.Element switch
        {
            ElementType.Fire => attacker.GetBCardInformation(BCardType.EnemyElementResistance, (byte)AdditionalTypes.EnemyElementResistance.FireDecreased).firstData,
            ElementType.Water => attacker.GetBCardInformation(BCardType.EnemyElementResistance, (byte)AdditionalTypes.EnemyElementResistance.WaterDecreased).firstData,
            ElementType.Light => attacker.GetBCardInformation(BCardType.EnemyElementResistance, (byte)AdditionalTypes.EnemyElementResistance.LightDecreased).firstData,
            ElementType.Shadow => attacker.GetBCardInformation(BCardType.EnemyElementResistance, (byte)AdditionalTypes.EnemyElementResistance.DarkDecreased).firstData,
            _ => 0
        };
        
        defenderResistance += attacker.GetBCardInformation(BCardType.EnemyElementResistance, (byte)AdditionalTypes.EnemyElementResistance.AllIncreased).firstData;
        defenderResistance += attacker.Element switch
        {
            ElementType.Fire => attacker.GetBCardInformation(BCardType.EnemyElementResistance, (byte)AdditionalTypes.EnemyElementResistance.FireIncreased).firstData,
            ElementType.Water => attacker.GetBCardInformation(BCardType.EnemyElementResistance, (byte)AdditionalTypes.EnemyElementResistance.WaterIncreased).firstData,
            ElementType.Light => attacker.GetBCardInformation(BCardType.EnemyElementResistance, (byte)AdditionalTypes.EnemyElementResistance.LightIncreased).firstData,
            ElementType.Shadow => attacker.GetBCardInformation(BCardType.EnemyElementResistance, (byte)AdditionalTypes.EnemyElementResistance.DarkIncreased).firstData,
            _ => 0
        };
        
        defenderResistance -= defender.GetBCardInformation(BCardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.AllDecreased).firstData;
        defenderResistance -= attacker.Element switch
        {
            ElementType.Fire => defender.GetBCardInformation(BCardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.FireDecreased).firstData,
            ElementType.Water => defender.GetBCardInformation(BCardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.WaterDecreased).firstData,
            ElementType.Light => defender.GetBCardInformation(BCardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.LightDecreased).firstData,
            ElementType.Shadow => defender.GetBCardInformation(BCardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.DarkDecreased).firstData,
            _ => 0
        };
        
        defenderResistance += defender.GetBCardInformation(BCardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.AllIncreased).firstData;
        defenderResistance += attacker.Element switch
        {
            ElementType.Fire => defender.GetBCardInformation(BCardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.FireIncreased).firstData,
            ElementType.Water => defender.GetBCardInformation(BCardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.WaterIncreased).firstData,
            ElementType.Light => defender.GetBCardInformation(BCardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.LightIncreased).firstData,
            ElementType.Shadow => defender.GetBCardInformation(BCardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.DarkIncreased).firstData,
            _ => 0
        };
        
        defenderResistance += defender.GetShellArmorEffectValue(ShellEffectType.IncreasedAllResistance);
        defenderResistance += attacker.Element switch
        {
            ElementType.Fire => defender.GetShellArmorEffectValue(ShellEffectType.IncreasedFireResistance),
            ElementType.Water => defender.GetShellArmorEffectValue(ShellEffectType.IncreasedWaterResistance),
            ElementType.Light => defender.GetShellArmorEffectValue(ShellEffectType.IncreasedLightResistance),
            ElementType.Shadow => defender.GetShellArmorEffectValue(ShellEffectType.IncreasedDarkResistance),
            _ => 0
        };
        
        defenderResistance += attacker.Element switch
        {
            ElementType.Fire => defender.GetFamilyUpgradeValue(FamilyUpgradeType.FIRE_RESISTANCE),
            ElementType.Water => defender.GetFamilyUpgradeValue(FamilyUpgradeType.WATER_RESISTANCE),
            ElementType.Light => defender.GetFamilyUpgradeValue(FamilyUpgradeType.LIGHT_RESISTANCE),
            ElementType.Shadow => defender.GetFamilyUpgradeValue(FamilyUpgradeType.DARK_RESISTANCE),
            _ => 0
        };
        
        if (isPvP)
        {
            defenderResistance -= attacker.GetShellWeaponEffectValue(ShellEffectType.ReducesEnemyAllResistancesInPVP);
            defenderResistance -= attacker.Element switch
            {
                ElementType.Fire => attacker.GetShellWeaponEffectValue(ShellEffectType.ReducesEnemyFireResistanceInPVP),
                ElementType.Water =>attacker.GetShellWeaponEffectValue(ShellEffectType.ReducesEnemyWaterResistanceInPVP),
                ElementType.Light => attacker.GetShellWeaponEffectValue(ShellEffectType.ReducesEnemyLightResistanceInPVP),
                ElementType.Shadow => attacker.GetShellWeaponEffectValue(ShellEffectType.ReducesEnemyDarkResistanceInPVP),
                _ => 0
            };
        }
        
        if (defender.HasBCard(BCardType.ConditionalEffects, (byte)AdditionalTypes.ConditionalEffects.IncreaseAllElementalResistancesIfBuffActive))
        {
            (int firstData, int secondData, _) = defender.GetBCardInformation(BCardType.ConditionalEffects,
                (byte)AdditionalTypes.ConditionalEffects.IncreaseAllElementalResistancesIfBuffActive);

            if (defender.HasBuff(secondData))
            {
                defenderResistance += defenderResistance * firstData / 100;
            }
        }
        
        if (defender.HasBCard(BCardType.NoCharacteristicValue, (byte)AdditionalTypes.NoCharacteristicValue.AllResistancesNullified))
        {
            defenderResistance = 0;
        }
        
        defenderResistance -= defender.GetFamilyUpgradeValue(FamilyUpgradeType.REDUCE_OPPONENTS_RES);
        
        if (defender.HasBCard(BCardType.RainbowBattleEffects, (byte)AdditionalTypes.RainbowBattleEffects.ElementalResistanceIncreaseInRainbowBattle) && defender.MapInstance.MapInstanceType == MapInstanceType.RainbowBattle)
        {
            int firstData = defender.GetBCardInformation(BCardType.RainbowBattleEffects, 
                (byte)AdditionalTypes.RainbowBattleEffects.ElementalResistanceIncreaseInRainbowBattle).firstData;

            defenderResistance += firstData;
        }
        
        if (defender.HasBCard(BCardType.RainbowBattleEffects, (byte)AdditionalTypes.RainbowBattleEffects.ElementalResistanceDecreaseInRainbowBattle) && defender.MapInstance.MapInstanceType == MapInstanceType.RainbowBattle)
        {
            int firstData = defender.GetBCardInformation(BCardType.RainbowBattleEffects, 
                (byte)AdditionalTypes.RainbowBattleEffects.ElementalResistanceDecreaseInRainbowBattle).firstData;

            defenderResistance -= firstData;
        }
        
        if (defender.MapInstance.GetCharacterById(defender.Id) is { SubClass: SubClassType.CelestialPaladin } celestialPaladin && (isPvE || isPvP))
        {
            int resistanceIncreaseRate = celestialPaladin.TierLevel switch
            {
                1 => 3, 
                2 => 4, 
                3 => 4, 
                4 => 5, 
                5 => 5, 
                _ => 0 
            };
            
            defenderResistance += resistanceIncreaseRate;
        }
        
        bool removeResistance = attacker.Element switch
        {
            ElementType.Fire => defender.HasBCard(BCardType.NoCharacteristicValue, (byte)AdditionalTypes.NoCharacteristicValue.FireResistanceNullified),
            ElementType.Water => defender.HasBCard(BCardType.NoCharacteristicValue, (byte)AdditionalTypes.NoCharacteristicValue.WaterResistanceNullified),
            ElementType.Light => defender.HasBCard(BCardType.NoCharacteristicValue, (byte)AdditionalTypes.NoCharacteristicValue.LightResistanceNullified),
            ElementType.Shadow => defender.HasBCard(BCardType.NoCharacteristicValue, (byte)AdditionalTypes.NoCharacteristicValue.DarkResistanceNullified),
            _ => false
        };
        
        if (removeResistance)
        {
            defenderResistance = 0;
        }

        double defenderResistanceMultiplier = 1;
        
        defenderResistanceMultiplier += defender.GetFirstDataMultiplier(BCardType.IncreaseElementByResis, (byte)AdditionalTypes.IncreaseElementByResis.AllElementResisIncrease);
        defenderResistanceMultiplier -= defender.GetFirstDataMultiplier(BCardType.IncreaseElementByResis, (byte)AdditionalTypes.IncreaseElementByResis.AllElementResisDecrease); 
        
        if (defender.MapInstance.HasMapFlag(MapFlags.IS_ACT4_DUNGEON) && defender.IsPlayer() && defender is not NpcMonsterEntityDump { MonsterRaceType: MonsterRaceType.People, MonsterRaceSubType: MonsterSubRace.People.Humanlike } or NpcMonsterEntityDump { MonsterRaceType: MonsterRaceType.HighLevel, MonsterRaceSubType: MonsterSubRace.HighLevel.Monster })
        {
            defenderResistanceMultiplier += defender.GetFirstDataMultiplier(BCardType.Act4DungeonElemental, (byte)AdditionalTypes.Act4DungeonElemental.ElementalResistancesIncrease);
            defenderResistanceMultiplier -= defender.GetFirstDataMultiplier(BCardType.Act4DungeonElemental, (byte)AdditionalTypes.Act4DungeonElemental.ElementalResistancesDecrease); 
        }

        defenderResistanceMultiplier += attacker.Element switch
        {
            ElementType.Fire => defender.GetFirstDataMultiplier(BCardType.IncreaseElementByResis, (byte)AdditionalTypes.IncreaseElementByResis.FireElementResisIncrease),
            ElementType.Water => defender.GetFirstDataMultiplier(BCardType.IncreaseElementByResis, (byte)AdditionalTypes.IncreaseElementByResis.WaterElementResisIncrease),
            ElementType.Light => defender.GetFirstDataMultiplier(BCardType.IncreaseElementByResis, (byte)AdditionalTypes.IncreaseElementByResis.LightElementResisIncrease), 
            ElementType.Shadow => defender.GetFirstDataMultiplier(BCardType.IncreaseElementByResis, (byte)AdditionalTypes.IncreaseElementByResis.ShadowElementResisIncrease),
            _ => 0
        };
        
        defenderResistanceMultiplier -= attacker.Element switch
        {
            ElementType.Fire => defender.GetFirstDataMultiplier(BCardType.IncreaseElementByResis, (byte)AdditionalTypes.IncreaseElementByResis.FireElementResisDecrease),
            ElementType.Water => defender.GetFirstDataMultiplier(BCardType.IncreaseElementByResis, (byte)AdditionalTypes.IncreaseElementByResis.WaterElementResisDecrease),
            ElementType.Light => defender.GetFirstDataMultiplier(BCardType.IncreaseElementByResis, (byte)AdditionalTypes.IncreaseElementByResis.LightElementResisDecrease),
            ElementType.Shadow => defender.GetFirstDataMultiplier(BCardType.IncreaseElementByResis, (byte)AdditionalTypes.IncreaseElementByResis.ShadowElementResisDecrease),
            _ => 0
        };
        
        defenderResistance = (int)(defenderResistance * defenderResistanceMultiplier);
        
        #endregion
        
        return new CalculationBasicStatistics
        {
            AttackerMorale = attackerMorale,
            AttackerAttackUpgrade = attackerAttackUpgrade,
            AttackerHitRate = attackerHitRate,
            AttackerCriticalChance = attackerCriticalChance,
            AttackerCriticalDamage = attackerCriticalDamage,
            AttackerElementRate = attackerElementRate,
            AttackerMagicalDefense = attackerMagicalDefense,
            DefenderMorale = defenderMorale,
            DefenderDefenseUpgrade = defenderDefenseUpgrade,
            DefenderDefense = defenderDefense,
            DefenderDefenseArmour = defenderDefenseArmour,
            DefenderDodge = defenderDodge,
            DefenderResistance = defenderResistance,
        };
    }

    public static CalculationPhysicalDamage CalculatePhysicalDamage(this IBattleEntityDump attacker, IBattleEntityDump defender, SkillInfo skill)
    {
        bool isPvP = attacker.IsPlayer() && defender.IsPlayer();
        
        double mapBaseDamageMultiplier = 1;
        
        #region Final Damage

        int finalDamage = attacker.TryFindPartnerSkillInformation(BCardType.AttackPower, (byte)AdditionalTypes.AttackPower.AllAttacksIncreased, skill).firstData;

        if (!defender.HasBCard(BCardType.VulcanoElementBuff, (byte)AdditionalTypes.VulcanoElementBuff.ReducesEnemyAttack))
        {
            finalDamage += attacker.AttackType switch
            {
                AttackType.Melee => attacker.TryFindPartnerSkillInformation(BCardType.AttackPower, (byte)AdditionalTypes.AttackPower.MeleeAttacksIncreased, skill).firstData,
                AttackType.Ranged => attacker.TryFindPartnerSkillInformation(BCardType.AttackPower, (byte)AdditionalTypes.AttackPower.RangedAttacksIncreased, skill).firstData,
                AttackType.Magical => attacker.TryFindPartnerSkillInformation(BCardType.AttackPower, (byte)AdditionalTypes.AttackPower.MagicalAttacksIncreased, skill).firstData,
                _ => 0
            };
        }
        
        finalDamage += attacker.GetFamilyUpgradeValue(FamilyUpgradeType.INCREASE_ALL_ATTACKS);

        finalDamage -= attacker.GetBCardInformation(BCardType.AttackPower, (byte)AdditionalTypes.AttackPower.AllAttacksDecreased).firstData;
        finalDamage -= attacker.AttackType switch
        {
            AttackType.Melee => attacker.GetBCardInformation(BCardType.AttackPower, (byte)AdditionalTypes.AttackPower.MeleeAttacksDecreased).firstData,
            AttackType.Ranged => attacker.GetBCardInformation(BCardType.AttackPower, (byte)AdditionalTypes.AttackPower.RangedAttacksDecreased).firstData,
            AttackType.Magical => attacker.GetBCardInformation(BCardType.AttackPower, (byte)AdditionalTypes.AttackPower.MagicalAttacksDecreased).firstData,
            _ => 0
        };
        
        if (attacker.HasBCard(BCardType.MineralTokenEffects, (byte)AdditionalTypes.MineralTokenEffects.SpendTokensIncreaseAttackPower))
        {
            var player = (IPlayerEntity)attacker.MapInstance.GetBattleEntity(attacker.Type, attacker.Id);

            (int firstData, int secondData, _) = attacker.GetBCardInformation(BCardType.MineralTokenEffects, (byte)AdditionalTypes.MineralTokenEffects.SpendTokensIncreaseAttackPower);
            
            if (player.TokenGauge >= firstData)
            {
                if (attacker.HasBCard(BCardType.TokenBasedAbilities, (byte)AdditionalTypes.TokenBasedAbilities.TokenEnhancementBuff))
                {
                    finalDamage += (int)(finalDamage * (secondData / 100.0));
                }
            }
        }
        
        if (attacker.HasBCard(BCardType.SpareBatteryTokenEffects, (byte)AdditionalTypes.SpareBatteryTokenEffects.SpareBatteryTokensIncreaseAttackPower))
        {
            var player = (IPlayerEntity)attacker.MapInstance.GetBattleEntity(attacker.Type, attacker.Id);

            (int firstData, int secondData, _) = attacker.GetBCardInformation(BCardType.SpareBatteryTokenEffects, (byte)AdditionalTypes.SpareBatteryTokenEffects.SpareBatteryTokensIncreaseAttackPower);
            
            if (player.TokenGauge >= firstData)
            {
                finalDamage += (int)(finalDamage * (secondData / 100.0));
            }
        }
        
                
        if (attacker.HasBCard(BCardType.LordMorcos, (byte)AdditionalTypes.LordMorcos.IncreaseSunChaserDamage))
        {
            var player = (IPlayerEntity)attacker.MapInstance.GetBattleEntity(attacker.Type, attacker.Id);
            
            IMateEntity sunWolf = player.MateComponent.GetMate(x => x.NpcMonsterVNum == (int)MonsterVnum.SUN_WOLF);
            (int firstData, int secondData) = player.BCardComponent.GetAllBCardsInformation(BCardType.LordMorcos, (byte)AdditionalTypes.LordMorcos.IncreaseSunChaserDamage, player.Level);

            int? hpPercent = sunWolf?.GetHpPercentage();
            if (hpPercent >= firstData)
            {
                finalDamage += secondData;
            }
        }
        
        if (defender.HasBCard(BCardType.Drain, (byte)AdditionalTypes.Drain.AttackedBySunWolfIncreaseDamage))
        {
            int firstData = defender.GetBCardInformation(BCardType.Drain, (byte)AdditionalTypes.Drain.AttackedBySunWolfIncreaseDamage).firstData;
            
            if (attacker.IsMate())
            {
                var sunWolf = (IMateEntity)attacker.MapInstance.GetBattleEntity(attacker.Type, attacker.Id);;
                if (sunWolf.NpcMonsterVNum is (short)MonsterVnum.SUN_WOLF)
                {
                    finalDamage += firstData;
                }
            }
        }
        
        if (attacker.HasBCard(BCardType.InflictSkill, (byte)AdditionalTypes.InflictSkill.AttackIncreasedByRageBar))
        {
            var player = (IPlayerEntity)attacker.MapInstance.GetBattleEntity(attacker.Type, attacker.Id);

            int firstData = player.BCardComponent.GetAllBCardsInformation(BCardType.InflictSkill,
                (byte)AdditionalTypes.InflictSkill.AttackIncreasedByRageBar, player.Level).firstData;

            finalDamage += player.EnergyBar * firstData;
        }
        
        if (skill != null && skill.BCards.Any(x => x.Type == (short)BCardType.ConditionalEffects && x.SubType == (byte)AdditionalTypes.ConditionalEffects.ConsumeFuelPointsIncreaseDamage))
        {
            var player = (IPlayerEntity)attacker.MapInstance.GetBattleEntity(attacker.Type, attacker.Id);
            
            (int firstData, int secondData, _) = attacker.GetBCardInformation(BCardType.ConditionalEffects, (byte)AdditionalTypes.ConditionalEffects.ConsumeFuelPointsIncreaseDamage);
            
            if (player.EnergyBar >= firstData)
            {
                finalDamage += (int)(finalDamage * secondData * 0.01);
                
                player.UpdateEnergyBar(-firstData).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
        
        if (attacker.HasBCard(BCardType.SummonSkill, (byte)AdditionalTypes.SummonSkill.IncreasesDamageIfWaterfallFrenzy))
        {
            var player = (IPlayerEntity)attacker.MapInstance.GetBattleEntity(attacker.Type, attacker.Id);
    
            (int firstData, int secondData, _) = attacker.GetBCardInformation(BCardType.SummonSkill, (byte)AdditionalTypes.SummonSkill.IncreasesDamageIfWaterfallFrenzy);

            if (player.HasBuff(BuffVnums.WATERFALL_FRENZY) && player.EnergyBar >= 90)
            {
                double damageIncreasePercentage = player.EnergyBar / 100.0 * (firstData / 100.0);
                finalDamage += (int)Math.Round(finalDamage * damageIncreasePercentage);

                player.PreventEnergyRemove = true;
                int energyReduction = (int)Math.Round(player.EnergyBar * (secondData / 100.0));
                player.UpdateEnergyBar(-energyReduction).ConfigureAwait(false).GetAwaiter().GetResult();
                player.PreventEnergyRemove = false;
            }
        }

        /*if (attacker.HasBCard(BCardType.DealDamageAround, (byte)AdditionalTypes.DealDamageAround.NosMateAttackIncrease) &&
            attacker.IsMate())
        {
            int firstData = attacker.GetBCardInformation(BCardType.DealDamageAround,
                (byte)AdditionalTypes.DealDamageAround.NosMateAttackIncrease).firstData;
            finalDamage += firstData * 0.01;
        }


        if (attacker.HasBCard(BCardType.DefencePiercing, (byte)AdditionalTypes.DefencePiercing.IncreaseDamageWithPiercedArmour))
        {
            if (defender.HasBCard(BCardType.DefencePiercing, (byte)AdditionalTypes.DefencePiercing.PiercesArmourIncreasesDamage))
            {
                finalDamage += attacker.GetMultiplier(attacker.GetBCardInformation(BCardType.DefencePiercing,
                    (byte)AdditionalTypes.DefencePiercing.IncreaseDamageWithPiercedArmour).firstData);
            }
        }

        if (attacker.HasBCard(BCardType.BlasterHeat, (byte)AdditionalTypes.BlasterHeat.IncreaseAttackPowerDependingOnHeatPoints))
        {
            (int firstData, int secondData, _) = attacker.GetBCardInformation(BCardType.BlasterHeat,
                (byte)AdditionalTypes.BlasterHeat.IncreaseAttackPowerDependingOnHeatPoints);

            var player = (IPlayerEntity)attacker.MapInstance.GetBattleEntity(attacker.Type, attacker.Id);

            double percentageIncrease = player.BlasterHeatCalculation(player.EnergyBar, 0, 100, firstData, secondData);
            finalDamage += percentageIncrease * 0.01;
        }

        if (attacker.HasBCard(BCardType.Energy, (byte)AdditionalTypes.Energy.ConsumeAllHeatPointsIncreaseDamage))
        {
            int firstData = attacker.GetBCardInformation(BCardType.Energy, (byte)AdditionalTypes.Energy.ConsumeAllHeatPointsIncreaseDamage).firstData;

            var player = (IPlayerEntity)attacker.MapInstance.GetBattleEntity(attacker.Type, attacker.Id);

            finalDamage += firstData * Math.Abs(player.EnergyRemoved) * 0.01 / 100.0;
            player.EnergyRemoved = 0;
        }

        if (attacker.HasBCard(BCardType.Energy, (byte)AdditionalTypes.Energy.IncreaseDamageByPointsConsumption))
        {
            int firstData = attacker.GetBCardInformation(BCardType.Energy,(byte)AdditionalTypes.Energy.IncreaseDamageByPointsConsumption).firstData;

            var player = (IPlayerEntity)attacker.MapInstance.GetBattleEntity(attacker.Type, attacker.Id);

            finalDamage += (player.EnergyBar + player.SecondEnergyBar * firstData) * 0.01 / 100.0;
            player.UpdateBothEnergyBars(-100, -100).ConfigureAwait(false).GetAwaiter().GetResult();
        }*/

        if (isPvP)
        {
            finalDamage += attacker.GetBCardInformation(BCardType.SpecialisationBuffResistance, (byte)AdditionalTypes.SpecialisationBuffResistance.IncreaseDamageInPVP).firstData;
            finalDamage -= attacker.GetBCardInformation(BCardType.SpecialisationBuffResistance, (byte)AdditionalTypes.SpecialisationBuffResistance.DecreaseDamageInPVP).firstData;
        }

        if (attacker.IsInvisible())
        {
            finalDamage += attacker.GetBCardInformation(BCardType.LightAndShadow, (byte)AdditionalTypes.LightAndShadow.AdditionalDamageWhenHidden).firstData;
        }
        
        finalDamage += attacker.GetShellWeaponEffectValue(ShellEffectType.DamageImproved);
        
        double badBuffStackAttack = 1 + attacker.GetBCardInformation(BCardType.IncreaseDamageDebuffs, (byte)AdditionalTypes.IncreaseDamageDebuffs.IncreasePowerOnDebuff).firstData * attacker.BadBuffCounter * 0.01;
        finalDamage = (int)(finalDamage * badBuffStackAttack);

        #endregion
        
        #region Damage Multiplier

        double damageMultiplier = 1;
        
        damageMultiplier += attacker.ChanceBCardMultiplier(BCardType.Critical, (byte)AdditionalTypes.Critical.DamageIncreasingChance);
        damageMultiplier -= attacker.ChanceBCardMultiplier(BCardType.Critical, (byte)AdditionalTypes.Critical.DamageReducingChance);
        damageMultiplier += attacker.GetFirstDataMultiplier(BCardType.Item, (byte)AdditionalTypes.Item.AttackIncreased);
        
        if (attacker.MapInstance.HasMapFlag(MapFlags.IS_ACT4_DUNGEON) && attacker.IsPlayer() && defender is not NpcMonsterEntityDump { MonsterRaceType: MonsterRaceType.People, MonsterRaceSubType: MonsterSubRace.People.Humanlike })
        {
            damageMultiplier += attacker.GetFirstDataMultiplier(BCardType.Act4DungeonElemental, (byte)AdditionalTypes.Act4DungeonElemental.AllAttacksIncrease);
            damageMultiplier -= attacker.GetFirstDataMultiplier(BCardType.Act4DungeonElemental, (byte)AdditionalTypes.Act4DungeonElemental.AllAttacksDecrease);
        }
        
        damageMultiplier += attacker.GetFamilyUpgradeValue(FamilyUpgradeType.INCREASE_ATTACK_DEFENSE) * 0.01;
        damageMultiplier += defender.GetFirstDataMultiplier(BCardType.MysticArts, (byte)AdditionalTypes.MysticArts.SignUseNextAttackIncrease);
        damageMultiplier -= defender.GetFirstDataMultiplier(BCardType.MysticArts, (byte)AdditionalTypes.MysticArts.SignUseNextDamageTakenDecrease);

        (int firstData, int secondData, int count) absorptionAttackIncreased = defender.GetBCardInformation(BCardType.Absorption, (byte)AdditionalTypes.Absorption.AllAttackIncreased);

        if (absorptionAttackIncreased.firstData != 0 && absorptionAttackIncreased.firstData > attacker.AttackUpgrade)
        {
            damageMultiplier += absorptionAttackIncreased.secondData * 0.01;
        }
        
        (int firstData, int secondData, int count) absorptionAttackDecreased = defender.GetBCardInformation(BCardType.Absorption, (byte)AdditionalTypes.Absorption.AllAttackDecreased);

        if (absorptionAttackDecreased.firstData != 0 && absorptionAttackDecreased.firstData > attacker.AttackUpgrade)
        {
            damageMultiplier -= absorptionAttackDecreased.secondData * 0.01;
        }

        absorptionAttackIncreased = attacker.AttackType switch
        {
            AttackType.Melee => defender.GetBCardInformation(BCardType.Absorption, (byte)AdditionalTypes.Absorption.MeleeAttackIncreased),
            AttackType.Ranged => defender.GetBCardInformation(BCardType.Absorption, (byte)AdditionalTypes.Absorption.RangedAttackIncreased),
            AttackType.Magical => defender.GetBCardInformation(BCardType.Absorption, (byte)AdditionalTypes.Absorption.MagicalAttackIncreased),
            _ => (0, 0, 0)
        };
        
        absorptionAttackDecreased = attacker.AttackType switch
        {
            AttackType.Melee => defender.GetBCardInformation(BCardType.Absorption, (byte)AdditionalTypes.Absorption.MeleeAttackDecreased),
            AttackType.Ranged => defender.GetBCardInformation(BCardType.Absorption, (byte)AdditionalTypes.Absorption.RangedAttackDecreased),
            AttackType.Magical => defender.GetBCardInformation(BCardType.Absorption, (byte)AdditionalTypes.Absorption.MagicalAttacksDecreased),
            _ => (0, 0, 0)
        };
        
        if (absorptionAttackIncreased.firstData != 0 && absorptionAttackIncreased.firstData > attacker.AttackUpgrade)
        {
            damageMultiplier += absorptionAttackIncreased.secondData * 0.01;
        }
        
        if (absorptionAttackDecreased.firstData != 0 && absorptionAttackDecreased.firstData > attacker.AttackUpgrade)
        {
            damageMultiplier -= absorptionAttackDecreased.secondData * 0.01;
        }

        double softDamageMultiplier = 0;
        
        softDamageMultiplier += attacker.ChanceBCardMultiplier(BCardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.IncreasingProbability);
        softDamageMultiplier -= attacker.ChanceBCardMultiplier(BCardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.DecreasingProbability);
        
        switch (attacker.Element)
        {
            case ElementType.Fire:
            {
                if (defender.HasBCard(BCardType.IncreaseDamageByElement, (byte)AdditionalTypes.IncreaseDamageByElement.FireElementIncreaseDamage))
                {
                    (int firstData, int secondData, _) = defender.GetBCardInformation(BCardType.IncreaseDamageByElement, 
                        (byte)AdditionalTypes.IncreaseDamageByElement.FireElementIncreaseDamage);
                    if (defender.IsSucceededChance(firstData))
                    {
                        softDamageMultiplier += secondData * 0.01;
                    }
                }
            
                if (defender.HasBCard(BCardType.IncreaseDamageByElement, (byte)AdditionalTypes.IncreaseDamageByElement.FireElementDecreaseDamage))
                {
                    (int firstData, int secondData, _) = defender.GetBCardInformation(BCardType.IncreaseDamageByElement, 
                        (byte)AdditionalTypes.IncreaseDamageByElement.FireElementDecreaseDamage);
                    if (defender.IsSucceededChance(firstData))
                    {
                        softDamageMultiplier -= secondData * 0.01;
                    }
                }

                break;
            }
            case ElementType.Water:
            {
                if (defender.HasBCard(BCardType.IncreaseDamageByElement, (byte)AdditionalTypes.IncreaseDamageByElement.WaterElementIncreaseDamage))
                {
                    (int firstData, int secondData, _) = defender.GetBCardInformation(BCardType.IncreaseDamageByElement, 
                        (byte)AdditionalTypes.IncreaseDamageByElement.WaterElementIncreaseDamage);
                    if (defender.IsSucceededChance(firstData))
                    {
                        softDamageMultiplier += secondData * 0.01;
                    }
                }
            
                if (defender.HasBCard(BCardType.IncreaseDamageByElement, (byte)AdditionalTypes.IncreaseDamageByElement.WaterElementDecreaseDamage))
                {
                    (int firstData, int secondData, _) = defender.GetBCardInformation(BCardType.IncreaseDamageByElement, 
                        (byte)AdditionalTypes.IncreaseDamageByElement.WaterElementDecreaseDamage);
                    if (defender.IsSucceededChance(firstData))
                    {
                        softDamageMultiplier -= secondData * 0.01;
                    }
                }

                break;
            }
            case ElementType.Light:
            {
                if (defender.HasBCard(BCardType.IncreaseDamageByElement, (byte)AdditionalTypes.IncreaseDamageByElement.LightElementIncreaseDamage))
                {
                    (int firstData, int secondData, _) = defender.GetBCardInformation(BCardType.IncreaseDamageByElement, 
                        (byte)AdditionalTypes.IncreaseDamageByElement.LightElementIncreaseDamage);
                    if (defender.IsSucceededChance(firstData))
                    {
                        softDamageMultiplier += secondData * 0.01;
                    }
                }
            
                if (defender.HasBCard(BCardType.IncreaseDamageByElement, (byte)AdditionalTypes.IncreaseDamageByElement.LightElementDecreaseDamage))
                {
                    (int firstData, int secondData, _) = defender.GetBCardInformation(BCardType.IncreaseDamageByElement, 
                        (byte)AdditionalTypes.IncreaseDamageByElement.LightElementDecreaseDamage);
                    if (defender.IsSucceededChance(firstData))
                    {
                        softDamageMultiplier -= secondData * 0.01;
                    }
                }
                
                break;
            }
            case ElementType.Shadow:
            {
                if (defender.HasBCard(BCardType.IncreaseDamageByElement, (byte)AdditionalTypes.IncreaseDamageByElement.DarkElementIncreaseDamage))
                {
                    (int firstData, int secondData, _) = defender.GetBCardInformation(BCardType.IncreaseDamageByElement, 
                        (byte)AdditionalTypes.IncreaseDamageByElement.DarkElementIncreaseDamage);
                    if (defender.IsSucceededChance(firstData))
                    {
                        softDamageMultiplier += secondData * 0.01;
                    }
                }
            
                if (defender.HasBCard(BCardType.IncreaseDamageByElement, (byte)AdditionalTypes.IncreaseDamageByElement.DarkElementDecreaseDamage))
                {
                    (int firstData, int secondData, _) = defender.GetBCardInformation(BCardType.IncreaseDamageByElement, 
                        (byte)AdditionalTypes.IncreaseDamageByElement.DarkElementDecreaseDamage);
                    if (defender.IsSucceededChance(firstData))
                    {
                        softDamageMultiplier -= secondData * 0.01;
                    }
                }
                break;
            }
        }

        double finalDamageMultiplier = 1;
        
        finalDamageMultiplier += attacker.GetShellWeaponEffectValue(ShellEffectType.PercentageTotalDamage) * 0.01;
        
        finalDamageMultiplier += attacker.GetFirstDataMultiplier(BCardType.IncreaseAllDamage, (byte)AdditionalTypes.IncreaseAllDamage.AllAttackIncrease);
        finalDamageMultiplier -= attacker.GetFirstDataMultiplier(BCardType.IncreaseAllDamage, (byte)AdditionalTypes.IncreaseAllDamage.AllAttackDecrease);

        finalDamageMultiplier += attacker.AttackType switch
        {
            AttackType.Melee => attacker.GetFirstDataMultiplier(BCardType.IncreaseAllDamage, (byte)AdditionalTypes.IncreaseAllDamage.MeleeAttackIncrease),
            AttackType.Ranged => attacker.GetFirstDataMultiplier(BCardType.IncreaseAllDamage, (byte)AdditionalTypes.IncreaseAllDamage.RangeAttackIncrease),
            AttackType.Magical => attacker.GetFirstDataMultiplier(BCardType.IncreaseAllDamage, (byte)AdditionalTypes.IncreaseAllDamage.MagicAttackIncrease),
            _ => 0
        };
        
        finalDamageMultiplier -= attacker.AttackType switch
        {
            AttackType.Melee => attacker.GetFirstDataMultiplier(BCardType.IncreaseAllDamage, (byte)AdditionalTypes.IncreaseAllDamage.MeleeAttackDecrease),
            AttackType.Ranged => attacker.GetFirstDataMultiplier(BCardType.IncreaseAllDamage, (byte)AdditionalTypes.IncreaseAllDamage.RangeAttackDecrease),
            AttackType.Magical => attacker.GetFirstDataMultiplier(BCardType.IncreaseAllDamage, (byte)AdditionalTypes.IncreaseAllDamage.MagicAttackDecrease),
            _ => 0
        };
        
        int multiplyDamageInt = 1;
        
        multiplyDamageInt += attacker.GetBCardInformation(BCardType.MultAttack, (byte)AdditionalTypes.MultAttack.AllAttackIncreased).firstData;
        multiplyDamageInt += attacker.AttackType switch
        {
            AttackType.Melee => attacker.GetBCardInformation(BCardType.MultAttack, (byte)AdditionalTypes.MultAttack.MeleeAttackIncreased).firstData,
            AttackType.Ranged => attacker.GetBCardInformation(BCardType.MultAttack, (byte)AdditionalTypes.MultAttack.RangedAttackIncreased).firstData,
            AttackType.Magical => attacker.GetBCardInformation(BCardType.MultAttack, (byte)AdditionalTypes.MultAttack.MagicalAttackIncreased).firstData,
            _ => 0
        };
        
        multiplyDamageInt -= attacker.GetBCardInformation(BCardType.MultAttack, (byte)AdditionalTypes.MultAttack.AllAttackDecreased).firstData;
        multiplyDamageInt -= attacker.AttackType switch
        {
            AttackType.Melee => attacker.GetBCardInformation(BCardType.MultAttack, (byte)AdditionalTypes.MultAttack.MeleeAttackDecreased).firstData,
            AttackType.Ranged => attacker.GetBCardInformation(BCardType.MultAttack, (byte)AdditionalTypes.MultAttack.RangedAttackDecreased).firstData,
            AttackType.Magical => attacker.GetBCardInformation(BCardType.MultAttack, (byte)AdditionalTypes.MultAttack.MagicalAttackDecreased).firstData,
            _ => 0
        };

        finalDamageMultiplier /= multiplyDamageInt;

        #endregion
        
        #region PVE Damage
        
        double pveDamageMultiplier = 1;

        if (defender.IsMonster())
        {
            pveDamageMultiplier += attacker.GetFirstDataMultiplier(BCardType.IncreaseElementFairy, (byte)AdditionalTypes.IncreaseElementFairy.DamageToMonstersIncrease);
            pveDamageMultiplier -= attacker.GetFirstDataMultiplier(BCardType.IncreaseElementFairy, (byte)AdditionalTypes.IncreaseElementFairy.DamageToMonstersDecrease);
            
            MonsterRaceType monsterRaceType = defender.MonsterRaceType;
            Enum monsterRaceSubType = defender.MonsterRaceSubType;

            int monsterRace;

            MonsterRaceType bCardRaceType;
            Enum bCardRaceSubType;
            if (attacker.HasBCard(BCardType.SpecialisationBuffResistance, (byte)AdditionalTypes.SpecialisationBuffResistance.IncreaseDamageAgainst))
            {
                (int firstData, int secondData, int count) raceBCard =
                    attacker.GetBCardInformation(BCardType.SpecialisationBuffResistance, (byte)AdditionalTypes.SpecialisationBuffResistance.IncreaseDamageAgainst);

                monsterRace = raceBCard.firstData;
                bCardRaceType = (MonsterRaceType)Math.Floor(monsterRace / 10.0);
                bCardRaceSubType = attacker.GetRaceSubType(bCardRaceType, (byte)(monsterRace % 10));

                if (monsterRaceType == bCardRaceType && bCardRaceSubType != null && Equals(monsterRaceSubType, bCardRaceSubType))
                {
                    finalDamage += raceBCard.secondData;
                }
            }
            
            if (attacker.HasBCard(BCardType.SpecialisationBuffResistance, (byte)AdditionalTypes.SpecialisationBuffResistance.ReduceDamageAgainst))
            {
                (int firstData, int secondData, int count) raceBCard =
                    attacker.GetBCardInformation(BCardType.SpecialisationBuffResistance, (byte)AdditionalTypes.SpecialisationBuffResistance.ReduceDamageAgainst);

                monsterRace = raceBCard.firstData;
                bCardRaceType = (MonsterRaceType)Math.Floor(monsterRace / 10.0);
                bCardRaceSubType = attacker.GetRaceSubType(bCardRaceType, (byte)(monsterRace % 10));

                if (monsterRaceType == bCardRaceType && bCardRaceSubType != null && Equals(monsterRaceSubType, bCardRaceSubType))
                {
                    finalDamage -= raceBCard.secondData;
                }
            }

            if (attacker.HasBCard(BCardType.LeonaPassiveSkill, (byte)AdditionalTypes.LeonaPassiveSkill.IncreaseDamageAgainst))
            {
                (int firstData, int secondData, int count) raceBCard =
                    attacker.GetBCardInformation(BCardType.LeonaPassiveSkill, (byte)AdditionalTypes.LeonaPassiveSkill.IncreaseDamageAgainst);

                monsterRace = raceBCard.firstData;
                bCardRaceType = (MonsterRaceType)Math.Floor(monsterRace / 10.0);
                bCardRaceSubType = attacker.GetRaceSubType(bCardRaceType, (byte)(monsterRace % 10));

                if (monsterRaceType == bCardRaceType && bCardRaceSubType != null && Equals(monsterRaceSubType, bCardRaceSubType))
                {
                    pveDamageMultiplier += raceBCard.secondData * 0.01;
                }
            }
            
            if (attacker.HasBCard(BCardType.LeonaPassiveSkill, (byte)AdditionalTypes.LeonaPassiveSkill.DecreaseDamageAgainst))
            {
                (int firstData, int secondData, int count) raceBCard =
                    attacker.GetBCardInformation(BCardType.LeonaPassiveSkill, (byte)AdditionalTypes.LeonaPassiveSkill.DecreaseDamageAgainst);

                monsterRace = raceBCard.firstData;
                bCardRaceType = (MonsterRaceType)Math.Floor(monsterRace / 10.0);
                bCardRaceSubType = attacker.GetRaceSubType(bCardRaceType, (byte)(monsterRace % 10));

                if (monsterRaceType == bCardRaceType && bCardRaceSubType != null && Equals(monsterRaceSubType, bCardRaceSubType))
                {
                    pveDamageMultiplier -= raceBCard.secondData * 0.01;
                }
            }

            pveDamageMultiplier += attacker.GetShellWeaponEffectValue(ShellEffectType.DamageIncreasedEnemy) * 0.01;
            
            if (attacker.HasBCard(BCardType.Target, (byte)AdditionalTypes.Target.IncreaseDamageAgainstMonster))
            {
                int firstData = attacker.GetBCardInformation(BCardType.Target, (byte)AdditionalTypes.Target.IncreaseDamageAgainstMonster).firstData;
                pveDamageMultiplier += attacker.GetMultiplier(firstData);
            }

            if (defender.MapInstance.GetMonsterById(defender.Id) != null)
            {
                IMonsterEntity monsterEntity = defender.MapInstance.GetMonsterById(defender.Id);
                if (monsterEntity.IsBoss || monsterEntity.DropToInventory)
                {
                    pveDamageMultiplier += attacker.GetShellWeaponEffectValue(ShellEffectType.DamageIncreasedBigMonster) * 0.01;
                    
                    if (!isPvP && defender.Level >= attacker.Level)
                    {
                        (int firstData, int secondData, int count) increaseDamageMonsterBCard =
                            attacker.GetBCardInformation(BCardType.EffectSummon, (byte)AdditionalTypes.EffectSummon.IfMobHigherLevelDamageIncrease);

                        if (attacker.IsSucceededChance(increaseDamageMonsterBCard.firstData))
                        {
                            pveDamageMultiplier += attacker.GetMultiplier(increaseDamageMonsterBCard.secondData);
                        }
                    }
                }
                else
                {
                    pveDamageMultiplier += attacker.GetShellWeaponEffectValue(ShellEffectType.DamageIncreasedSmallMonster) * 0.01;
                }
            }
            switch (monsterRaceSubType)
            {
                case MonsterSubRace.HighLevel.Dragon:
                    pveDamageMultiplier += attacker.GetFamilyUpgradeValue(FamilyUpgradeType.INCREASE_DMG_TO_HLDRAGONS) * 0.01;
                    break;
                case MonsterSubRace.LowLevel.Animal:
                    
                    pveDamageMultiplier += attacker.GetShellWeaponEffectValue(ShellEffectType.DamageIncreasedAnimal) * 0.01;
                    pveDamageMultiplier += attacker.GetBCardInformation(BCardType.IncreaseDamageVersusMonsters, (byte)AdditionalTypes.IncreaseDamageVersusMonsters.LowLevelAnimalAttackIncrease).firstData * 0.01;
                    pveDamageMultiplier -= attacker.GetBCardInformation(BCardType.IncreaseDamageVersusMonsters, (byte)AdditionalTypes.IncreaseDamageVersusMonsters.LowLevelAnimalAttackDecrease).firstData * 0.01;
                    break;
                case MonsterSubRace.HighLevel.Animal:
                    pveDamageMultiplier += attacker.GetShellWeaponEffectValue(ShellEffectType.DamageIncreasedAnimal) * 0.01;
                    break;
                case MonsterSubRace.LowLevel.Plant:
                    pveDamageMultiplier += attacker.GetShellWeaponEffectValue(ShellEffectType.DamageIncreasedPlant) * 0.01;
                    pveDamageMultiplier += attacker.GetBCardInformation(BCardType.IncreaseDamageVersusMonsters_3, (byte)AdditionalTypes.IncreaseDamageVersusMonsters_3.PlantAttackIncrease).firstData;
                    pveDamageMultiplier -= attacker.GetBCardInformation(BCardType.IncreaseDamageVersusMonsters_3, (byte)AdditionalTypes.IncreaseDamageVersusMonsters_3.PlantAttackDecrease).firstData;
                    pveDamageMultiplier += attacker.GetBCardInformation(BCardType.IncreaseDamageVersusMonsters, (byte)AdditionalTypes.IncreaseDamageVersusMonsters.LowLevelPlantAttackIncrease).firstData * 0.01;
                    pveDamageMultiplier -= attacker.GetBCardInformation(BCardType.IncreaseDamageVersusMonsters, (byte)AdditionalTypes.IncreaseDamageVersusMonsters.LowLevelPlantAttackDecrease).firstData * 0.01;
                    break;
                case MonsterSubRace.HighLevel.Plant:
                    pveDamageMultiplier += attacker.GetShellWeaponEffectValue(ShellEffectType.DamageIncreasedPlant) * 0.01;
                    pveDamageMultiplier += attacker.GetBCardInformation(BCardType.IncreaseDamageVersusMonsters_3, (byte)AdditionalTypes.IncreaseDamageVersusMonsters_3.PlantAttackIncrease).firstData;
                    pveDamageMultiplier -= attacker.GetBCardInformation(BCardType.IncreaseDamageVersusMonsters_3, (byte)AdditionalTypes.IncreaseDamageVersusMonsters_3.PlantAttackDecrease).firstData;
                    break;
                case MonsterSubRace.LowLevel.Monster:
                    pveDamageMultiplier += attacker.GetBCardInformation(BCardType.IncreaseDamageVersusMonsters, (byte)AdditionalTypes.IncreaseDamageVersusMonsters.LowLevelMonsterAttackIncrease).firstData * 0.01;
                    pveDamageMultiplier -= attacker.GetBCardInformation(BCardType.IncreaseDamageVersusMonsters, (byte)AdditionalTypes.IncreaseDamageVersusMonsters.LowLevelMonsterAttackDecrease).firstData * 0.01;
                    break;
                case MonsterSubRace.HighLevel.Monster:
                    break;
                case MonsterSubRace.Undead.LowLevelUndead:
                    pveDamageMultiplier += attacker.GetShellWeaponEffectValue(ShellEffectType.DamageIncreasedUnDead) * 0.01;
                    pveDamageMultiplier += attacker.GetBCardInformation(BCardType.IncreaseDamageVersusMonsters_2, (byte)AdditionalTypes.IncreaseDamageVersusMonsters_2.LowLevelUndeadAttackIncrease).firstData * 0.01;
                    pveDamageMultiplier -= attacker.GetBCardInformation(BCardType.IncreaseDamageVersusMonsters_2, (byte)AdditionalTypes.IncreaseDamageVersusMonsters_2.LowLevelUndeadAttackDecrease).firstData * 0.01;
                    break;
                case MonsterSubRace.Undead.HighLevelUndead:
                    pveDamageMultiplier += attacker.GetShellWeaponEffectValue(ShellEffectType.DamageIncreasedUnDead) * 0.01;
                    break;
                case MonsterSubRace.Undead.Vampire:
                    pveDamageMultiplier += attacker.GetShellWeaponEffectValue(ShellEffectType.DamageIncreasedUnDead) * 0.01;
                    break;
                case MonsterSubRace.Spirits.LowLevelGhost:
                    break;
                case MonsterSubRace.Spirits.HighLevelGhost:
                    break;
                case MonsterSubRace.Spirits.LowLevelSpirit:
                    pveDamageMultiplier += attacker.GetBCardInformation(BCardType.IncreaseDamageVersusMonsters_2, (byte)AdditionalTypes.IncreaseDamageVersusMonsters_2.LowLevelSpiritAttackIncrease).firstData * 0.01;
                    pveDamageMultiplier -= attacker.GetBCardInformation(BCardType.IncreaseDamageVersusMonsters_2, (byte)AdditionalTypes.IncreaseDamageVersusMonsters_2.LowLevelSpiritAttackDecrease).firstData * 0.01;
                    break;
                case MonsterSubRace.Spirits.HighLevelSpirit:
                    break;
                
                case MonsterSubRace.Angels.Angel:
                    pveDamageMultiplier += attacker.GetBCardInformation(BCardType.IncreaseDamageVersusMonsters_2, (byte)AdditionalTypes.IncreaseDamageVersusMonsters_2.AngelAttackIncrease).firstData * 0.01;
                    pveDamageMultiplier -= attacker.GetBCardInformation(BCardType.IncreaseDamageVersusMonsters_2, (byte)AdditionalTypes.IncreaseDamageVersusMonsters_2.AngelAttackDecrease).firstData * 0.01;
                    break;

                case MonsterSubRace.Angels.Demon:
                    pveDamageMultiplier += attacker.GetBCardInformation(BCardType.IncreaseDamageVersusMonsters_2, (byte)AdditionalTypes.IncreaseDamageVersusMonsters_2.DemonAttackIncrease).firstData * 0.01;
                    pveDamageMultiplier -= attacker.GetBCardInformation(BCardType.IncreaseDamageVersusMonsters_2, (byte)AdditionalTypes.IncreaseDamageVersusMonsters_2.DemonAttackDecrease).firstData * 0.01;
                    break;
                
                case MonsterSubRace.Furry.Kovolt:
                    pveDamageMultiplier += attacker.GetBCardInformation(BCardType.IncreaseDamageVersusMonsters, (byte)AdditionalTypes.IncreaseDamageVersusMonsters.KovoltAttackIncrease).firstData * 0.01;
                    pveDamageMultiplier -= attacker.GetBCardInformation(BCardType.IncreaseDamageVersusMonsters, (byte)AdditionalTypes.IncreaseDamageVersusMonsters.KovoltAttackDecrease).firstData * 0.01;
                    break;

                case MonsterSubRace.Furry.Catsy:
                    pveDamageMultiplier += attacker.GetBCardInformation(BCardType.IncreaseDamageVersusMonsters, (byte)AdditionalTypes.IncreaseDamageVersusMonsters.CatsyAttackIncrease).firstData * 0.01;
                    pveDamageMultiplier -= attacker.GetBCardInformation(BCardType.IncreaseDamageVersusMonsters, (byte)AdditionalTypes.IncreaseDamageVersusMonsters.CatsyAttackDecrease).firstData * 0.01;
                    break;
            }
            
            if (defender is NpcMonsterEntityDump { IsVesselMonster: true } or NpcMonsterEntityDump { IsVesselChristmasMonster: true})
            {
                pveDamageMultiplier += attacker.GetFirstDataMultiplier(BCardType.IncreaseDamageVersus, (byte)AdditionalTypes.IncreaseDamageVersus.VesselAndLodMobDamageIncrease);  // 90-21
                pveDamageMultiplier += attacker.GetFirstDataMultiplier(BCardType.IncreaseDamageVersus, (byte)AdditionalTypes.IncreaseDamageVersus.VesselAndFrozenCrownMobDamageIncrease);  // 90-41
                pveDamageMultiplier += attacker.GetFirstDataMultiplier(BCardType.IncreaseDamageInLoD, (byte)AdditionalTypes.IncreaseDamageInLoD.VesselMonstersAttackIncrease);  // 101-21
            }
            
            if (defender is NpcMonsterEntityDump { MonsterRaceType: MonsterRaceType.HighLevel, MonsterRaceSubType: MonsterSubRace.HighLevel.Dragon })
            {
                (int firstData, int secondData, int count) damageToDragonBCardIncreased = attacker.GetBCardInformation(BCardType.DamageInflict, (byte)AdditionalTypes.DamageInflict.DamageToDragonIncreased);
                pveDamageMultiplier += attacker.GetMultiplier(damageToDragonBCardIncreased.firstData);
                (int firstData, int secondData, int count) damageToDragonBCardDecreased = attacker.GetBCardInformation(BCardType.DamageInflict, (byte)AdditionalTypes.DamageInflict.DamageToDragonDecreased);
                pveDamageMultiplier -= attacker.GetMultiplier(damageToDragonBCardDecreased.firstData);
            }

            if (attacker.MapInstance.MapId == (short)MapIds.LAND_OF_DEATH)
            {
                mapBaseDamageMultiplier += attacker.GetBCardInformation(BCardType.IncreaseDamageVersus, (byte)AdditionalTypes.IncreaseDamageVersus.VesselAndLodMobDamageIncrease).secondData * 0.01;  // 90-21
                mapBaseDamageMultiplier += attacker.GetFirstDataMultiplier(BCardType.IncreaseDamageInLoD, (byte)AdditionalTypes.IncreaseDamageInLoD.LodMonstersAttackIncrease);  // 101-11
            }
            
            if (attacker.MapInstance.MapId is (short)MapIds.LAND_OF_LIFE or (short)MapIds.ADVANCED_LAND_OF_LIFE ||
                attacker.MapInstance.HasMapFlag(MapFlags.ACT_8))
            {
                mapBaseDamageMultiplier += attacker.GetFirstDataMultiplier(BCardType.DodgeAndDefencePercent, (byte)AdditionalTypes.DodgeAndDefencePercent.LolMonstersAttackIncrease);
                mapBaseDamageMultiplier -= attacker.GetFirstDataMultiplier(BCardType.DodgeAndDefencePercent, (byte)AdditionalTypes.DodgeAndDefencePercent.LolMonstersAttackDecrease);
            
            }
            
            if (defender is NpcMonsterEntityDump { MonsterRaceType: MonsterRaceType.People, MonsterRaceSubType: MonsterSubRace.People.Humanlike } || 
                (defender is NpcMonsterEntityDump { MonsterRaceType: MonsterRaceType.HighLevel, MonsterRaceSubType: MonsterSubRace.HighLevel.Monster } && 
                    attacker.MapInstance.HasMapFlag(MapFlags.IS_ACT4_DUNGEON) && attacker.IsPlayer()))
            {
                mapBaseDamageMultiplier += attacker.GetFirstDataMultiplier(BCardType.Act4DungeonElemental, (byte)AdditionalTypes.Act4DungeonElemental.DamageToRaidBossesIncrease);
                mapBaseDamageMultiplier -= attacker.GetFirstDataMultiplier(BCardType.Act4DungeonElemental, (byte)AdditionalTypes.Act4DungeonElemental.DamageToRaidBossesDecrease);
            }

        }

        #endregion
        
        #region PVP Damage

        double pvpDamageMultiplier = 1;

        if (isPvP)
        {
            switch (attacker.Faction)
            {
                case FactionType.Demon:
                    pvpDamageMultiplier += attacker.GetFirstDataMultiplier(BCardType.ChangingPlace, (byte)AdditionalTypes.ChangingPlace.IncreaseDamageVersusAngels);
                    pvpDamageMultiplier -= attacker.GetFirstDataMultiplier(BCardType.ChangingPlace, (byte)AdditionalTypes.ChangingPlace.DecreaseDamageVersusAngels);
                    break;
                
                case FactionType.Angel:
                    pvpDamageMultiplier += attacker.GetFirstDataMultiplier(BCardType.ChangingPlace, (byte)AdditionalTypes.ChangingPlace.IncreaseDamageVersusDemons);
                    pvpDamageMultiplier -= attacker.GetFirstDataMultiplier(BCardType.ChangingPlace, (byte)AdditionalTypes.ChangingPlace.DecreaseDamageVersusDemons);
                    break;
            }
            
            pvpDamageMultiplier += attacker.GetFirstDataMultiplier(BCardType.LeonaPassiveSkill, (byte)AdditionalTypes.LeonaPassiveSkill.AttackIncreasedInPVP);
            pvpDamageMultiplier -= attacker.GetFirstDataMultiplier(BCardType.LeonaPassiveSkill, (byte)AdditionalTypes.LeonaPassiveSkill.AttackDecreasedInPVP);
            pvpDamageMultiplier += attacker.GetShellWeaponEffectValue(ShellEffectType.PercentageDamageInPVP) * 0.01;


            
            var player = (IPlayerEntity)attacker.MapInstance.GetBattleEntity(attacker.Type, attacker.Id);
            pvpDamageMultiplier += player.StatisticsComponent.Passives.GetValueOrDefault(PassiveType.PVP_DAMAGE_HERO_BOOK) * 0.01;

            if (attacker.MapInstance.MapInstanceType == MapInstanceType.Alzanor)
            {
                pvpDamageMultiplier += attacker.GetFirstDataMultiplier(BCardType.CustomEventPercentageDamage, (byte)AdditionalTypes.CustomEventPercentageDamage.IncreaseDamageAlzanorBattle);
                pvpDamageMultiplier += attacker.GetFirstDataMultiplier(BCardType.CustomEventPercentageDamage, (byte)AdditionalTypes.CustomEventPercentageDamage.IncreaseDamageAlzanorBattleNegated);
            }

            if (attacker.MapInstance.MapInstanceType == MapInstanceType.RainbowBattle)
            {
                pvpDamageMultiplier += attacker.GetBCardInformation(BCardType.IncreaseDamageVersus, (byte)AdditionalTypes.IncreaseDamageVersus.PvpDamageAndSpeedRainbowBattleIncrease).firstData * 0.01;
                
                pvpDamageMultiplier += attacker.GetBCardInformation(BCardType.RainbowBattleEffects, (byte)AdditionalTypes.RainbowBattleEffects.DamageIncreaseInRainbowBattle).firstData * 0.01;
                pvpDamageMultiplier -= attacker.GetBCardInformation(BCardType.RainbowBattleEffects, (byte)AdditionalTypes.RainbowBattleEffects.DamageDecreaseInRainbowBattle).firstData * 0.01;
                
                pvpDamageMultiplier += defender.GetBCardInformation(BCardType.RainbowBattleEffects, (byte)AdditionalTypes.RainbowBattleEffects.DamageReceivedIncreaseInRainbowBattle).firstData * 0.01;
                pvpDamageMultiplier -= defender.GetBCardInformation(BCardType.RainbowBattleEffects, (byte)AdditionalTypes.RainbowBattleEffects.DamageReceivedDecreaseInRainbowBattle).firstData * 0.01;
            }

            if (attacker.MapInstance.MapInstanceType == MapInstanceType.TalentArena)
            {
                pvpDamageMultiplier += attacker.GetFirstDataMultiplier(BCardType.TeamArenaBuff, (byte)AdditionalTypes.TeamArenaBuff.AttackPowerIncreased);
                pvpDamageMultiplier -= attacker.GetFirstDataMultiplier(BCardType.TeamArenaBuff, (byte)AdditionalTypes.TeamArenaBuff.AttackPowerDecreased);
                pvpDamageMultiplier -= defender.GetFirstDataMultiplier(BCardType.TeamArenaBuff, (byte)AdditionalTypes.TeamArenaBuff.DamageTakenIncreased);
                pvpDamageMultiplier += defender.GetFirstDataMultiplier(BCardType.TeamArenaBuff, (byte)AdditionalTypes.TeamArenaBuff.DamageTakenDecreased);
            }
        }

        #endregion
        
        #region Physical Damage

        double physicalDamageMultiplier = 1;
        double physicalDamageMultiplierByType = 1;
        
        physicalDamageMultiplier += defender.GetFirstDataMultiplier(BCardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased);
        physicalDamageMultiplier -= defender.GetFirstDataMultiplier(BCardType.Damage, (byte)AdditionalTypes.Damage.DamageDecreased);
        
        physicalDamageMultiplierByType += attacker.AttackType switch
        {
            AttackType.Melee => defender.GetFirstDataMultiplier(BCardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased),
            AttackType.Ranged => defender.GetFirstDataMultiplier(BCardType.Damage, (byte)AdditionalTypes.Damage.RangedIncreased),
            AttackType.Magical => defender.GetFirstDataMultiplier(BCardType.Damage, (byte)AdditionalTypes.Damage.MagicalIncreased),
            _ => 0
        };

        physicalDamageMultiplierByType -= attacker.AttackType switch
        {
            AttackType.Melee => defender.GetFirstDataMultiplier(BCardType.Damage, (byte)AdditionalTypes.Damage.MeleeDecreased),
            AttackType.Ranged => defender.GetFirstDataMultiplier(BCardType.Damage, (byte)AdditionalTypes.Damage.RangedDecreased),
            AttackType.Magical => defender.GetFirstDataMultiplier(BCardType.Damage, (byte)AdditionalTypes.Damage.MagicalDecreased),
            _ => 0
        };
        
        if (attacker.HasBCard(BCardType.MultAttack, (byte)AdditionalTypes.MultAttack.DamageDecreasedByMissingHP))
        {
            int firstData = attacker.GetBCardInformation(BCardType.MultAttack, (byte)AdditionalTypes.MultAttack.DamageDecreasedByMissingHP).firstData;
            int missingHp = 100 - attacker.GetHpPercentage();
            double decreaseDamage = (double)missingHp / firstData * 0.01;
            physicalDamageMultiplier -= decreaseDamage;
        }
        
        if (defender.HasBCard(BCardType.TeamArenaBuff, (byte)AdditionalTypes.TeamArenaBuff.HPDropsBelowReduceDamage))
        {
            (int firstData, int secondData, _) = defender.GetBCardInformation(BCardType.TeamArenaBuff, (byte)AdditionalTypes.TeamArenaBuff.HPDropsBelowReduceDamage);
            int currentHpPercentage = defender.GetHpPercentage();
            
            if (currentHpPercentage < firstData)
            {
                physicalDamageMultiplier -= defender.GetMultiplier(secondData); 
            }
        }
                
        if (attacker.HasBCard(BCardType.MultAttack, (byte)AdditionalTypes.MultAttack.DamageIncreasedByMissingHP))
        {
            int lowerHpStrongerDamage = attacker.GetBCardInformation(BCardType.MultAttack, (byte)AdditionalTypes.MultAttack.DamageIncreasedByMissingHP).firstData;
            int missingHp = 100 - attacker.GetHpPercentage();
            
            double damageBoost = physicalDamageMultiplier * ((double)missingHp / lowerHpStrongerDamage * 0.01);
            physicalDamageMultiplier += damageBoost;
        }
        
        if (attacker.MapInstance.GetBattleEntity(attacker.Type, attacker.Id) is IPlayerEntity staticBonusPlayer && staticBonusPlayer.HaveStaticBonus(StaticBonusType.HolidaySpiritMedal))
        {
            physicalDamageMultiplier += 0.03;
        }
        
        if (attacker.HasBCard(BCardType.TeamArenaBuff, (byte)AdditionalTypes.TeamArenaBuff.HPDropsBelowIncreaseDamage))
        {
            (int firstData, int secondData, _) = attacker.GetBCardInformation(BCardType.TeamArenaBuff, (byte)AdditionalTypes.TeamArenaBuff.HPDropsBelowIncreaseDamage);
            int currentHpPercentage = attacker.GetHpPercentage();
            
            if (currentHpPercentage < firstData)
            {
                physicalDamageMultiplier += attacker.GetMultiplier(secondData);
            }
        }

        #endregion
        
        #region Damage By Magic Defense

        double magicDefenseAttackMultiplier = 0;
        
        magicDefenseAttackMultiplier += attacker.GetFirstDataMultiplier(BCardType.ReflectDamage, (byte)AdditionalTypes.ReflectDamage.AllAttackIncreasePerMagicDefense);

        #endregion
        
        #region Increase Attack By Skill

        double skillBasedDamageMultiplier = 1;
        
        if (attacker.HasBCard(BCardType.AbsorptionAndPowerSkill, (byte)AdditionalTypes.AbsorptionAndPowerSkill.DamageIncreasedSkill))
        {
            (int firstData, int secondData, _) = attacker.GetBCardInformation(BCardType.AbsorptionAndPowerSkill, (byte)AdditionalTypes.AbsorptionAndPowerSkill.DamageIncreasedSkill);
            if (skill.Vnum == firstData)
            {
                skillBasedDamageMultiplier += secondData * 0.01;
            }
        }
        
        if (attacker.HasBCard(BCardType.AbsorptionAndPowerSkill, (byte)AdditionalTypes.AbsorptionAndPowerSkill.DamageDecreasedSkill))
        {
            (int firstData, int secondData, _) = attacker.GetBCardInformation(BCardType.AbsorptionAndPowerSkill, (byte)AdditionalTypes.AbsorptionAndPowerSkill.DamageDecreasedSkill);
            if (skill.Vnum == firstData)
            {
                skillBasedDamageMultiplier -= secondData * 0.01;
            }
        }

        if (skill != null)
        {
            if (skill.IsComboSkill && defender.HasBCard(BCardType.DamageConvertingSkill, (byte)AdditionalTypes.DamageConvertingSkill.AdditionalDamageCombo))
            {
                skillBasedDamageMultiplier += defender.GetFirstDataMultiplier(BCardType.DamageConvertingSkill, (byte)AdditionalTypes.DamageConvertingSkill.AdditionalDamageCombo);
            }
        }

        #endregion
        
        #region Increase by Element
        
        double elementBasedDamageMultiplier = 1;
        double elementalDamageMultiplier = 1;
        double elementalSoftDamageMultiplier = 1;
        double elementBasedDefenseMultiplier = 1;
        
        elementBasedDefenseMultiplier += defender.ChanceBCardMultiplier(BCardType.IncreaseElementDamage, (byte)AdditionalTypes.IncreaseElementDamage.AllElementAttackIncrease);
        elementBasedDefenseMultiplier -= defender.ChanceBCardMultiplier(BCardType.IncreaseElementDamage, (byte)AdditionalTypes.IncreaseElementDamage.AllElementAttackDecrease);

        switch (attacker.Element)
        {
            case ElementType.Fire:
            {
                elementBasedDefenseMultiplier += defender.ChanceBCardMultiplier(BCardType.IncreaseElementDamage, (byte)AdditionalTypes.IncreaseElementDamage.FireElementAttackIncrease); // 98-21
                elementBasedDefenseMultiplier -= defender.ChanceBCardMultiplier(BCardType.IncreaseElementDamage, (byte)AdditionalTypes.IncreaseElementDamage.FireElementAttackDecrease); // 98-22
                break;
            }
            case ElementType.Water:
            {
                elementBasedDefenseMultiplier += defender.ChanceBCardMultiplier(BCardType.IncreaseElementDamage, (byte)AdditionalTypes.IncreaseElementDamage.WaterElementAttackIncrease); // 98-31
                elementBasedDefenseMultiplier -= defender.ChanceBCardMultiplier(BCardType.IncreaseElementDamage, (byte)AdditionalTypes.IncreaseElementDamage.WaterElementAttackDecrease); // 98-32
                break;
            }
            case ElementType.Light:
            {
                elementBasedDefenseMultiplier += defender.ChanceBCardMultiplier(BCardType.IncreaseElementDamage, (byte)AdditionalTypes.IncreaseElementDamage.LightElementAttackIncrease); // 98-41
                elementBasedDefenseMultiplier -= defender.ChanceBCardMultiplier(BCardType.IncreaseElementDamage, (byte)AdditionalTypes.IncreaseElementDamage.LightElementAttackDecrease); // 98-42
                break;
            }
            case ElementType.Shadow:
            {
                elementalDamageMultiplier += defender.ChanceBCardMultiplier(BCardType.DarkCloneSummon, (byte)AdditionalTypes.DarkCloneSummon.DarkElementDamageIncreaseChance); // 80-51
                elementBasedDefenseMultiplier += defender.ChanceBCardMultiplier(BCardType.IncreaseElementDamage, (byte)AdditionalTypes.IncreaseElementDamage.ShadowElementAttackIncrease); // 98-51
                elementBasedDefenseMultiplier -= defender.ChanceBCardMultiplier(BCardType.IncreaseElementDamage, (byte)AdditionalTypes.IncreaseElementDamage.ShadowElementAttackDecrease); // 98-52
                break;
            }
        }
        
        #endregion

        return new CalculationPhysicalDamage
        {
            FinalAttack = finalDamage,
            FinalAttackMultiplier = finalDamageMultiplier,
            DamageMultiplier = damageMultiplier,
            SoftDamageMultiplier = softDamageMultiplier,
            MapBasedDamageMultiplier = mapBaseDamageMultiplier,
            PhysicalDamageMultiplier = physicalDamageMultiplier,
            PhysicalDamageMultiplierByType = physicalDamageMultiplierByType,
            MagicDefenseAttackMultiplier = magicDefenseAttackMultiplier,
            PveDamageMultiplier = pveDamageMultiplier,
            PvpDamageMultiplier = pvpDamageMultiplier,
            SkillBasedDamageMultiplier = skillBasedDamageMultiplier,
            ElementBasedDamageMultiplier = elementBasedDamageMultiplier,
            ElementalDamageMultiplier = elementalDamageMultiplier,
            ElementalSoftDamageMultiplier = elementalSoftDamageMultiplier,
            ElementBasedDefenseMultiplier = elementBasedDefenseMultiplier,
        };
    }

    public static CalculationElementDamage CalculateElementDamage(this IBattleEntityDump attacker, IBattleEntityDump defender, SkillInfo skill)
    {
        int element = 0;
        double elementMultiplier = 1;
        double elementMultiply = 0;
        
        if (attacker.HasBCard(BCardType.NoCharacteristicValue, (byte)AdditionalTypes.NoCharacteristicValue.AllPowersNullified))
        {
            return new CalculationElementDamage();
        }

        bool turnOffElement = attacker.Element switch
        {
            ElementType.Fire => attacker.HasBCard(BCardType.NoCharacteristicValue, (byte)AdditionalTypes.NoCharacteristicValue.FireElementNullified),
            ElementType.Water => attacker.HasBCard(BCardType.NoCharacteristicValue, (byte)AdditionalTypes.NoCharacteristicValue.WaterElementNullified),
            ElementType.Light => attacker.HasBCard(BCardType.NoCharacteristicValue, (byte)AdditionalTypes.NoCharacteristicValue.LightElementNullified),
            ElementType.Shadow => attacker.HasBCard(BCardType.NoCharacteristicValue, (byte)AdditionalTypes.NoCharacteristicValue.DarkElementNullified),
            _ => true
        };

        if (turnOffElement)
        {
            return new CalculationElementDamage();
        }
        
        switch (attacker.Element)
        {
            case ElementType.Fire:
                element += attacker.TryFindPartnerSkillInformation(BCardType.Element, (byte)AdditionalTypes.Element.FireIncreased, skill).firstData;
                element -= attacker.GetBCardInformation(BCardType.Element, (byte)AdditionalTypes.Element.FireDecreased).firstData;
                element += attacker.GetBCardInformation(BCardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.FireIncreased).firstData;
                element -= attacker.GetBCardInformation(BCardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.FireDecreased).firstData;
                element += attacker.GetShellWeaponEffectValue(ShellEffectType.IncreasedFireProperties);
                element += (int)(element * attacker.GetFirstDataMultiplier(BCardType.IncreaseElementProcent, (byte)AdditionalTypes.IncreaseElementProcent.FireElementIncrease));
                element -= (int)(element * attacker.GetFirstDataMultiplier(BCardType.IncreaseElementProcent, (byte)AdditionalTypes.IncreaseElementProcent.FireElementDecrease));
                break;
            case ElementType.Water:
                element += attacker.TryFindPartnerSkillInformation(BCardType.Element, (byte)AdditionalTypes.Element.WaterIncreased, skill).firstData;
                element -= attacker.GetBCardInformation(BCardType.Element, (byte)AdditionalTypes.Element.WaterDecreased).firstData;
                element += attacker.GetBCardInformation(BCardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.WaterIncreased).firstData;
                element -= attacker.GetBCardInformation(BCardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.WaterDecreased).firstData;
                element += attacker.GetShellWeaponEffectValue(ShellEffectType.IncreasedWaterProperties);
                element += (int)(element * attacker.GetFirstDataMultiplier(BCardType.IncreaseElementProcent, (byte)AdditionalTypes.IncreaseElementProcent.WaterElementIncrease));
                element -= (int)(element * attacker.GetFirstDataMultiplier(BCardType.IncreaseElementProcent, (byte)AdditionalTypes.IncreaseElementProcent.WaterElementDecrease));
                break;
            case ElementType.Light:
                element += attacker.TryFindPartnerSkillInformation(BCardType.Element, (byte)AdditionalTypes.Element.LightIncreased, skill).firstData;
                element -= attacker.GetBCardInformation(BCardType.Element, (byte)AdditionalTypes.Element.LightDecreased).firstData;
                element += attacker.GetBCardInformation(BCardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.LightIncreased).firstData;
                element -= attacker.GetBCardInformation(BCardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.LightDecreased).firstData;
                element += attacker.GetShellWeaponEffectValue(ShellEffectType.IncreasedLightProperties);
                element += (int)(element * attacker.GetFirstDataMultiplier(BCardType.IncreaseElementProcent, (byte)AdditionalTypes.IncreaseElementProcent.LightElementIncrease));
                element -= (int)(element * attacker.GetFirstDataMultiplier(BCardType.IncreaseElementProcent, (byte)AdditionalTypes.IncreaseElementProcent.LightElementDecrease));
                break;
            case ElementType.Shadow:
                element += attacker.TryFindPartnerSkillInformation(BCardType.Element, (byte)AdditionalTypes.Element.DarkIncreased, skill).firstData;
                element -= attacker.GetBCardInformation(BCardType.Element, (byte)AdditionalTypes.Element.DarkDecreased).firstData;
                element += attacker.GetBCardInformation(BCardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.DarkIncreased).firstData;
                element -= attacker.GetBCardInformation(BCardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.DarkDecreased).firstData;
                element += attacker.GetShellWeaponEffectValue(ShellEffectType.IncreasedDarkProperties);
                element += (int)(element * attacker.GetFirstDataMultiplier(BCardType.IncreaseElementProcent, (byte)AdditionalTypes.IncreaseElementProcent.ShadowElementIncrease));
                element -= (int)(element * attacker.GetFirstDataMultiplier(BCardType.IncreaseElementProcent, (byte)AdditionalTypes.IncreaseElementProcent.ShadowElementDecrease));
                break;
            case ElementType.Neutral:
                return new CalculationElementDamage();
        }

        element += attacker.GetBCardInformation(BCardType.Element, (byte)AdditionalTypes.Element.AllIncreased).firstData;
        element -= attacker.GetBCardInformation(BCardType.Element, (byte)AdditionalTypes.Element.AllDecreased).firstData;
        element += attacker.GetShellWeaponEffectValue(ShellEffectType.IncreasedElementalProperties);
        element += (int)(element * attacker.GetFirstDataMultiplier(BCardType.IncreaseElementProcent, (byte)AdditionalTypes.IncreaseElementProcent.AllElementIncrease));
        element -= (int)(element * attacker.GetFirstDataMultiplier(BCardType.IncreaseElementProcent, (byte)AdditionalTypes.IncreaseElementProcent.AllElementDecrease));
        
        element = (int)(element * elementMultiplier);
        
        elementMultiply = attacker.Element switch
        {
            ElementType.Fire => defender.Element switch
            {
                ElementType.Fire => 1,
                ElementType.Water => 2,
                ElementType.Light => 1,
                ElementType.Shadow => 1.5,
                _ => 1.3
            },
            ElementType.Water => defender.Element switch
            {
                ElementType.Fire => 2,
                ElementType.Water => 1,
                ElementType.Light => 1.5,
                ElementType.Shadow => 1,
                _ => 1.3
            },
            ElementType.Light => defender.Element switch
            {
                ElementType.Fire => 1.5,
                ElementType.Water => 1,
                ElementType.Light => 1,
                ElementType.Shadow => 3,
                _ => 1.3
            },
            ElementType.Shadow => defender.Element switch
            {
                ElementType.Fire => 1,
                ElementType.Water => 1.5,
                ElementType.Light => 3,
                ElementType.Shadow => 1,
                _ => 1.3
            },
            _ => elementMultiply
        };

        return new CalculationElementDamage
        {
            Element = element,
            ElementMultiplier = elementMultiplier,
            ElementMultiply = elementMultiply
        };
    }

    public static CalculationDefense CalculationDefense(this IBattleEntityDump attacker, IBattleEntityDump defender)
    {
        bool isPvP = attacker.IsPlayer() && defender.IsPlayer();
        
        double softDefenseMultiplier = 1;
        double softDefenseByTypeMultiplier = 1;
        double receivedDamageMultiplier = 1;
        
        softDefenseMultiplier += defender.ChanceBCardMultiplier(BCardType.Block, (byte)AdditionalTypes.Block.ChanceAllIncreased);
        softDefenseMultiplier -= defender.ChanceBCardMultiplier(BCardType.Block, (byte)AdditionalTypes.Block.ChanceAllDecreased);
        
        switch (attacker.AttackType)
        {
            case AttackType.Melee:
                softDefenseByTypeMultiplier += defender.ChanceBCardMultiplier(BCardType.Block, (byte)AdditionalTypes.Block.ChanceMeleeIncreased);
                break;
            case AttackType.Ranged:
                softDefenseByTypeMultiplier += defender.ChanceBCardMultiplier(BCardType.Block, (byte)AdditionalTypes.Block.ChanceRangedIncreased);
                break;
            case AttackType.Magical:
                softDefenseByTypeMultiplier += defender.ChanceBCardMultiplier(BCardType.Block, (byte)AdditionalTypes.Block.ChanceMagicalIncreased);
                break;
        }
        
        (int firstData, int secondData, int _) increaseDefenseAttackTypeBCard;
        switch (attacker.AttackType)
        {
            case AttackType.Melee when defender.HasBCard(BCardType.Block, (byte)AdditionalTypes.Block.ChanceMeleeDecreased):
                increaseDefenseAttackTypeBCard = defender.GetBCardInformation(BCardType.Block, (byte)AdditionalTypes.Block.ChanceMeleeDecreased);
                if (defender.IsSucceededChance(increaseDefenseAttackTypeBCard.firstData))
                {
                    softDefenseByTypeMultiplier -= increaseDefenseAttackTypeBCard.secondData * 0.01;
                    defender.BroadcastEffect(EffectType.MeleeDefense);
                }

                break;
            case AttackType.Ranged when defender.HasBCard(BCardType.Block, (byte)AdditionalTypes.Block.ChanceRangedDecreased):
                increaseDefenseAttackTypeBCard = defender.GetBCardInformation(BCardType.Block, (byte)AdditionalTypes.Block.ChanceRangedDecreased);
                if (defender.IsSucceededChance(increaseDefenseAttackTypeBCard.firstData))
                {
                    softDefenseByTypeMultiplier -= increaseDefenseAttackTypeBCard.secondData * 0.01;
                    defender.BroadcastEffect(EffectType.RangeDefense);
                }

                break;
            case AttackType.Magical when defender.HasBCard(BCardType.Block, (byte)AdditionalTypes.Block.ChanceMagicalDecreased):
                increaseDefenseAttackTypeBCard = defender.GetBCardInformation(BCardType.Block, (byte)AdditionalTypes.Block.ChanceMagicalDecreased);
                if (defender.IsSucceededChance(increaseDefenseAttackTypeBCard.firstData))
                {
                    softDefenseByTypeMultiplier -= increaseDefenseAttackTypeBCard.secondData * 0.01;
                    defender.BroadcastEffect(EffectType.MagicDefense);
                }

                break;
        }
        
        receivedDamageMultiplier -= defender.GetFirstDataMultiplier(BCardType.Item, (byte)AdditionalTypes.Item.DefenceIncreased);
        receivedDamageMultiplier -= defender.GetFamilyUpgradeValue(FamilyUpgradeType.INCREASE_ATTACK_DEFENSE) * 0.01;
        
        if (defender.MapInstance.HasMapFlag(MapFlags.IS_ACT4_DUNGEON) && defender.IsPlayer() && defender is not NpcMonsterEntityDump { MonsterRaceType: MonsterRaceType.People, MonsterRaceSubType: MonsterSubRace.People.Humanlike } or NpcMonsterEntityDump { MonsterRaceType: MonsterRaceType.HighLevel, MonsterRaceSubType: MonsterSubRace.HighLevel.Monster })
        {
            receivedDamageMultiplier -= defender.GetFirstDataMultiplier(BCardType.Act4DungeonElemental, (byte)AdditionalTypes.Act4DungeonElemental.AllDefencesIncrease);
            receivedDamageMultiplier += defender.GetFirstDataMultiplier(BCardType.Act4DungeonElemental, (byte)AdditionalTypes.Act4DungeonElemental.AllDefencesDecrease);
        }
        
        if (defender.MapInstance.GetBattleEntity(defender.Type, defender.Id) is IPlayerEntity staticBonusPlayer && staticBonusPlayer.HaveStaticBonus(StaticBonusType.SugarStaffMedal))
        {
            receivedDamageMultiplier += 0.03;
        }
        
        double receivedPvpDamageMultiplier = 1;
        if (isPvP)
        {
            receivedPvpDamageMultiplier -= defender.GetFirstDataMultiplier(BCardType.LeonaPassiveSkill, (byte)AdditionalTypes.LeonaPassiveSkill.DefenceIncreasedInPVP); // 71-41
            receivedPvpDamageMultiplier += attacker.GetFirstDataMultiplier(BCardType.LeonaPassiveSkill, (byte)AdditionalTypes.LeonaPassiveSkill.DefenceDecreasedInPVP);  // 71-42
        }
        
        double defenseMultiplier = 1;
        defenseMultiplier += defender.GetFirstDataMultiplier(BCardType.DodgeAndDefencePercent, (byte)AdditionalTypes.DodgeAndDefencePercent.DefenceIncreased);
        defenseMultiplier -= defender.GetFirstDataMultiplier(BCardType.DodgeAndDefencePercent, (byte)AdditionalTypes.DodgeAndDefencePercent.DefenceReduced);
        
        defenseMultiplier += defender.GetShellArmorEffectValue(ShellEffectType.PercentageTotalDefence) * 0.01;
        if (isPvP)
        {
            defenseMultiplier -= attacker.GetShellWeaponEffectValue(ShellEffectType.ReducesPercentageEnemyDefenceInPVP) * 0.01;
            defenseMultiplier += defender.GetShellArmorEffectValue(ShellEffectType.PercentageAllPVPDefence) * 0.01;
        }
        
        double multiplyDefenseInt = defender.GetBCardInformation(BCardType.MultDefence, (byte)AdditionalTypes.MultDefence.AllDefenceIncreased).firstData;
        multiplyDefenseInt += attacker.AttackType switch
        {
            AttackType.Melee => defender.GetBCardInformation(BCardType.MultDefence, (byte)AdditionalTypes.MultDefence.MeleeDefenceIncreased).firstData,
            AttackType.Ranged => defender.GetBCardInformation(BCardType.MultDefence, (byte)AdditionalTypes.MultDefence.RangedDefenceIncreased).firstData,
            AttackType.Magical => defender.GetBCardInformation(BCardType.MultDefence, (byte)AdditionalTypes.MultDefence.MagicalDefenceIncreased).firstData,
            _ => 0
        };
        
        if (defender.HasBCard(BCardType.MultDefence, (byte)AdditionalTypes.MultDefence.DamageReducedPerDebuff))
        {
            IBattleEntity target = defender.MapInstance.GetBattleEntity(defender.Type, defender.Id);
            
            IReadOnlyList<Buff> debuffs = target.BuffComponent.GetAllBuffs(x => x.BuffGroup == BuffGroup.Bad) 
                ?? new List<Buff>();
    
            (int firstData, int secondData) = target.BCardComponent.GetAllBCardsInformation(BCardType.MultDefence,
                (byte)AdditionalTypes.MultDefence.DamageReducedPerDebuff, target.Level);

            int debuffCount = debuffs.Count;
            int defence = (int)(firstData * debuffCount * 0.01);
            defence = defence > secondData * 0.01 ? (int)(secondData * 0.01) : defence;
            multiplyDefenseInt += defence;
        }
        
        if (attacker.HasBCard(BCardType.MultAttack, (byte)AdditionalTypes.MultDefence.DamageIncreasedPerDebuff))
        {
            IBattleEntity target = attacker.MapInstance.GetBattleEntity(attacker.Type, attacker.Id);
            
            if (target is { BuffComponent: not null, BCardComponent: not null })
            {
                IReadOnlyList<Buff> debuffs = target.BuffComponent.GetAllBuffs(x => x.BuffGroup == BuffGroup.Bad);

                (int firstData, int secondData) = target.BCardComponent.GetAllBCardsInformation(
                    BCardType.MultDefence,
                    (byte)AdditionalTypes.MultDefence.DamageReducedPerDebuff,
                    target.Level
                );

                int debuffCount = debuffs.Count;
                int defence = (int)(firstData * debuffCount * 0.01);
                defence = defence > secondData * 0.01 ? (int)(secondData * 0.01) : defence;
                multiplyDefenseInt += defence;
            }
        }
        
        if (attacker.HasBCard(BCardType.StealBuff, (byte)AdditionalTypes.StealBuff.IgnoreDefenceChance))
        {
            (int firstData, int secondData, _) = attacker.GetBCardInformation(BCardType.StealBuff, (byte)AdditionalTypes.StealBuff.IgnoreDefenceChance);
            if (attacker.IsSucceededChance(firstData))
            {
                defenseMultiplier -= secondData * 0.01;
                attacker.BroadcastEffect(EffectType.IgnoreDefence);
            }
        }
        
        if (attacker.HasBCard(BCardType.FuelHeatPoint, (byte)AdditionalTypes.FuelHeatPoint.ConsumeFuelPointsIgnoreOpponentDefence))
        {
            var player = (IPlayerEntity)attacker.MapInstance.GetBattleEntity(attacker.Type, attacker.Id);
            
            (int firstData, int secondData, _) = attacker.GetBCardInformation(BCardType.FuelHeatPoint, (byte)AdditionalTypes.FuelHeatPoint.ConsumeFuelPointsIgnoreOpponentDefence);

            if (player.EnergyBar >= firstData)
            {
                defenseMultiplier -= secondData * 0.01;
                player.UpdateEnergyBar(-firstData).ConfigureAwait(false).GetAwaiter().GetResult();
                attacker.BroadcastEffect(EffectType.IgnoreDefence);
            }
        }
        
        defenseMultiplier += multiplyDefenseInt + 1;
        
        multiplyDefenseInt = defender.GetBCardInformation(BCardType.MultDefence, (byte)AdditionalTypes.MultDefence.AllDefenceDecreased).firstData;
        multiplyDefenseInt -= attacker.AttackType switch
        {
            AttackType.Melee => defender.GetBCardInformation(BCardType.MultDefence, (byte)AdditionalTypes.MultDefence.MeleeDefenceDecreased).firstData,
            AttackType.Ranged => defender.GetBCardInformation(BCardType.MultDefence, (byte)AdditionalTypes.MultDefence.RangedDefenceDecreased).firstData,
            AttackType.Magical => defender.GetBCardInformation(BCardType.MultDefence, (byte)AdditionalTypes.MultDefence.MagicalDefenceDecreased).firstData,
            _ => 0
        };
        
        defenseMultiplier /= multiplyDefenseInt + 1;

        return new CalculationDefense
        {
            SoftDefenseMultiplier = softDefenseMultiplier,
            SoftDefenseByTypeMultiplier = softDefenseByTypeMultiplier,
            ReceivedDamageMultiplier = receivedDamageMultiplier,
            ReceivedPvpDamageMultiplier = receivedPvpDamageMultiplier,
            DefenseMultiplier = defenseMultiplier
        };
    }

    public static CalculationResult CalculationResult(this IBattleEntityDump attacker, IBattleEntityDump defender, CalculationBasicStatistics basicStatistics,
        CalculationDefense defense, CalculationPhysicalDamage physicalDamage, CalculationElementDamage elementDamage, SkillInfo skill)
    {
        bool isPvP = attacker.IsPlayer() && defender.IsPlayer();
        bool isPvE = attacker.IsMonster() && defender.IsPlayer();
        bool isPvPAndPvE = defender.IsMonster() || defender.IsPlayer();
        bool isHighMonsterDamage = false;

        #region Attacker

        int attackerMorale = basicStatistics.AttackerMorale;
        int attackerAttackUpgrade = basicStatistics.AttackerAttackUpgrade;
        int attackerCriticalChance = basicStatistics.AttackerCriticalChance;
        int attackerMagicalDefense = basicStatistics.AttackerMagicalDefense;
        double attackerCriticalDamage = basicStatistics.AttackerCriticalDamage * 0.01;
        double attackerElementRate = basicStatistics.AttackerElementRate * 0.01;

        ElementType attackerElement = attacker.Element;
        AttackType attackType = attacker.AttackType;

        int damageMinimum = attacker.DamageMinimum;
        int damageMaximum = attacker.DamageMaximum;
        int weaponDamageMinimum = attacker.WeaponDamageMinimum;
        int weaponDamageMaximum = attacker.WeaponDamageMaximum;

        #endregion

        #region Defender

        int defenderMorale = basicStatistics.DefenderMorale;
        int defenderDefenseUpgrade = basicStatistics.DefenderDefenseUpgrade;
        int defenderDefense = basicStatistics.DefenderDefense;
        int defenderDefenseArmour = basicStatistics.DefenderDefenseArmour;
        int defenderResistance = basicStatistics.DefenderResistance;
        if (attacker.MapInstance.MapInstanceType == MapInstanceType.RaidInstance)
        {
            if (defender.IsMonster())
            {
                IMonsterEntity monsterEntity = attacker.MapInstance.GetMonsterById(defender.Id);
                if (monsterEntity is { IsBoss: true })
                {
                    switch (monsterEntity.MonsterVNum)
                    {
                        case 286:
                        {
                            defenderDefense /= 2;
                            defenderDefenseArmour /= 2;
                        }break;
                        case 289:
                        {
                            defenderDefense /= 2;
                            defenderDefenseArmour /= 2;
                        }break;
                        case 285:
                        {
                            defenderDefense /= 2;
                            defenderDefenseArmour /= 2;
                        }break;
                    }
                }
            }
        }
        #endregion

        #region Defense

        double softDefenseMultiplier = defense.SoftDefenseMultiplier;
        double softDefenseByTypeMultiplier = defense.SoftDefenseByTypeMultiplier;
        double receivedDamageMultiplier = defense.ReceivedDamageMultiplier;
        double receivedPvPDamageMultiplier = defense.ReceivedPvpDamageMultiplier;
        double defenseMultiplier = defense.DefenseMultiplier;

        #endregion

        #region Physical Damage

        int finalAttack = physicalDamage.FinalAttack;
        double damageMultiplier = physicalDamage.DamageMultiplier;
        double softDamageMultiplier = physicalDamage.SoftDamageMultiplier;
        double finalAttackMultiplier = physicalDamage.FinalAttackMultiplier;
        double mapBasedDamageMultiplier = physicalDamage.MapBasedDamageMultiplier;
        double physicalDamageMultiplier = physicalDamage.PhysicalDamageMultiplier;
        double physicalDamageMultiplierByType = physicalDamage.PhysicalDamageMultiplierByType;
        double magicDefenseAttackMultiplier = physicalDamage.MagicDefenseAttackMultiplier;
        double pveDamageMultiplier = physicalDamage.PveDamageMultiplier;
        double pvpDamageMultiplier = physicalDamage.PvpDamageMultiplier;
        double skillBasedDamageMultiplier = physicalDamage.SkillBasedDamageMultiplier;
        double elementBasedDamageMultiplier = physicalDamage.ElementBasedDamageMultiplier;
        double elementalDamageMultiplier = physicalDamage.ElementalDamageMultiplier;
        double elementalSoftDamageMultiplier = physicalDamage.ElementalSoftDamageMultiplier;
        double elementBasedDefenseMultiplier = physicalDamage.ElementBasedDefenseMultiplier;

        #endregion
        
        #region Basic Damage

        bool isCritical = attackType != AttackType.Magical && attackerCriticalChance > 0 && attacker.IsSucceededChance(attackerCriticalChance);
        bool isSoftDamage = softDamageMultiplier > 0;

        if (attacker.HasBCard(BCardType.Mode, (byte)AdditionalTypes.Mode.EffectNoDamage))
        {
            return new CalculationResult(0, false, false);
        }

        if (isCritical)
        {
            damageMinimum = (damageMinimum + damageMaximum) / 2;
            weaponDamageMinimum = (weaponDamageMinimum + weaponDamageMaximum) / 2;
        }

        #endregion
        
        #region Equipment

        int plusDifference = attackerAttackUpgrade - defenderDefenseUpgrade;
        int additionalDefense = 0;
			
        switch (plusDifference)
        {
            case > 0:
            {
                if (plusDifference > 10)
                {
                    plusDifference = 10;
                }

                weaponDamageMinimum += (int)(weaponDamageMinimum * Plus[plusDifference]);
                weaponDamageMaximum += (int)(weaponDamageMaximum * Plus[plusDifference]);
                break;
            }
            case < 0:
            {
                plusDifference = Math.Abs(plusDifference);
                if (plusDifference > 10)
                {
                    plusDifference = 10;
                }
                additionalDefense += (int)Math.Floor(defenderDefenseArmour * Plus[plusDifference]);
                break;
            }
        }

        #endregion
        
        #region Normal Damage

        finalAttack += RandomGenerator.RandomNumber(damageMinimum + weaponDamageMinimum, damageMaximum + weaponDamageMaximum) + 15;
        finalAttack = (int)(finalAttack * finalAttackMultiplier);

        if (magicDefenseAttackMultiplier > 0)
        {
            finalAttack += (int)Math.Floor(attackerMagicalDefense * magicDefenseAttackMultiplier);
        }

        int normalDmg = finalAttack - defenderDefense - additionalDefense;

        #endregion
        
        #region Critical Damage

        int criticalDmg = 0;
        if (isCritical)
        {
            criticalDmg += (int)Math.Floor(normalDmg * attackerCriticalDamage);
        }

        #endregion

        #region Element Damage

        double defenderResistanceMultiplier = defenderResistance >= 100 ? 0 : 1.0 - defenderResistance * 0.01;
        int element = elementDamage.Element;
        double elementMultiply = elementDamage.ElementMultiply;

        if (skill != null && skill.Element != (byte)attackerElement)
        {
            attackerElementRate = 0;
        }

        int elementDmg = 0;
        if (attackerElement != ElementType.Neutral)
        {
            elementDmg += (int)Math.Floor((finalAttack + 100) * attackerElementRate);
        }
			
        int elementSoft = 0;
        if (attackerElement != ElementType.Neutral && isSoftDamage)
        {
            elementSoft += (int)Math.Floor(finalAttack * softDamageMultiplier * attackerElementRate);
        }

        int elementalDamage = element + elementDmg + elementSoft;
        if (elementalDamage > 0)
        {
            elementalDamage = (int)Math.Floor(elementalDamage * elementMultiply);
            elementalDamage = (int)Math.Floor(elementalDamage * defenderResistanceMultiplier);
            elementalDamage = (int)Math.Floor(elementalDamage * elementalDamageMultiplier);
            elementalDamage = (int)Math.Floor(elementalDamage * elementalSoftDamageMultiplier);
        }
        else
        {
            elementalDamage = 0;
        }

        #endregion
        
        #region Physical Soft Damage

        int physicalSoft = 0;
        if (isSoftDamage)
        {
            physicalSoft += (int)Math.Floor(finalAttack * softDamageMultiplier);
        }

        #endregion
        
        #region Critical Soft Damage
		
        int criticalSoft = 0;
        if (isCritical && isSoftDamage)
        {
            criticalSoft += (int)Math.Floor(finalAttack * softDamageMultiplier * attackerCriticalDamage);
        }

        #endregion
        
        #region Physical Damage Defense

        int defenceByLevel;
        if (defender.IsPlayer())
        {
            defenceByLevel = -15;
        }
        else
        {
            defenceByLevel = attacker.GetMonsterDamageBonus(defender.Level);
        }
        
        int defenderDefenseDamage = defenderDefense + additionalDefense;
        defenderDefenseDamage = (int)Math.Floor(defenderDefenseDamage * defenseMultiplier);
        defenderDefenseDamage += defenceByLevel;
        
        #endregion

        #region Final Physical Damage
        
        double magicalDamageReduction = defender is PlayerBattleEntityDump playerBattleEntityDump && attackType == AttackType.Magical ? playerBattleEntityDump.DecreaseMagicDamage : 1;

        int physicalDmg = finalAttack + criticalDmg + physicalSoft + criticalSoft + attackerMorale - defenderMorale;
        physicalDmg -= defenderDefenseDamage <= 0 ? 0 : defenderDefenseDamage;
        physicalDmg = (int)Math.Floor(physicalDmg * physicalDamageMultiplier);
        physicalDmg = (int)Math.Floor(physicalDmg * physicalDamageMultiplierByType);
        physicalDmg = (int)Math.Floor(physicalDmg * magicalDamageReduction);
        physicalDmg = attacker.Penalties(defender, physicalDmg);
	    
        if (physicalDmg < 0)
        {
            physicalDmg = 0;
        }

        physicalDmg += GetMonsterBaseDamage(attacker, defender); 
        
        #endregion
        
        #region Final Critical Damage Multipliers
        
        double finalCriticalDamageMultiplier = 1;
        IBattleEntity attackEntity = attacker.MapInstance.GetBattleEntity(attacker.Type, attacker.Id);

        if (isCritical && attackEntity != null)
        {
            finalCriticalDamageMultiplier += attacker.GetFirstDataMultiplier(BCardType.Count, (byte)AdditionalTypes.Count.IncreaseFinalCriticalDamage);
            finalCriticalDamageMultiplier -= attacker.GetFirstDataMultiplier(BCardType.Count, (byte)AdditionalTypes.Count.DecreaseFinalCriticalDamage);
            finalCriticalDamageMultiplier += defender.GetFirstDataMultiplier(BCardType.Count, (byte)AdditionalTypes.Count.OnDefenceIncreaseCriticalDamage);
            finalCriticalDamageMultiplier -= defender.GetFirstDataMultiplier(BCardType.Count, (byte)AdditionalTypes.Count.OnDefenceDecreaseCriticalDamage);
        }
        
        # endregion
        
        #region Final Damage Multipliers

        double finalDamageMultiplier = 1;
        finalDamageMultiplier *= mapBasedDamageMultiplier;
        finalDamageMultiplier *= pveDamageMultiplier;
        finalDamageMultiplier *= pvpDamageMultiplier;
        finalDamageMultiplier *= skillBasedDamageMultiplier;
        finalDamageMultiplier *= elementBasedDamageMultiplier;
        finalDamageMultiplier *= elementBasedDefenseMultiplier;
        finalDamageMultiplier *= damageMultiplier;
        finalDamageMultiplier *= receivedDamageMultiplier;
        finalDamageMultiplier *= receivedPvPDamageMultiplier;
        finalDamageMultiplier *= softDefenseMultiplier;
        finalDamageMultiplier *= softDefenseByTypeMultiplier;
        finalDamageMultiplier *= finalCriticalDamageMultiplier;
        
        #endregion
        
        #region Charge Damage

        if (attackEntity != null)
        {
            int increaseDamageByCharge = 0;

            if (attackEntity.ChargeComponent.GetCharge() != 0)
            {
                increaseDamageByCharge += attackEntity.ChargeComponent.GetCharge();
                attackEntity.ChargeComponent.ResetCharge();
            }

            physicalDmg += increaseDamageByCharge;
        }

        #endregion
        
        int finalDamage = (int)((physicalDmg + elementalDamage) * finalDamageMultiplier);
        
        #region Damage Setters

        if (attacker.HasBCard(BCardType.RecoveryAndDamagePercent, (byte)AdditionalTypes.RecoveryAndDamagePercent.DecreaseEnemyHP))
        {
            double maxHpPercentage = attacker.GetFirstDataMultiplier(BCardType.RecoveryAndDamagePercent, (byte)AdditionalTypes.RecoveryAndDamagePercent.DecreaseEnemyHP);  // 37-31
            finalDamage = (int)(defender.MaxHp * maxHpPercentage);
        }
        
        if (defender.HasBCard(BCardType.RecoveryAndDamagePercent, (byte)AdditionalTypes.RecoveryAndDamagePercent.DecreaseEnemyHP))
        {
            double maxHpPercentage = defender.GetFirstDataMultiplier(BCardType.RecoveryAndDamagePercent, (byte)AdditionalTypes.RecoveryAndDamagePercent.DecreaseEnemyHP);  // 37-31
            finalDamage = (int)(defender.MaxHp * maxHpPercentage);
        }

        if (defender.HasBCard(BCardType.RecoveryAndDamagePercent, (byte)AdditionalTypes.RecoveryAndDamagePercent.DecreaseSelfHP))
        {
            double maxHpPercentage = defender.GetFirstDataMultiplier(BCardType.RecoveryAndDamagePercent, (byte)AdditionalTypes.RecoveryAndDamagePercent.DecreaseSelfHP);  // 37-32
            finalDamage = (int)(defender.MaxHp * maxHpPercentage);
        }
        
        int maximumCriticalDamage = defender.GetBCardInformation(BCardType.VulcanoElementBuff, (byte)AdditionalTypes.VulcanoElementBuff.CriticalDefence).firstData;  // 66-41
        if (isCritical && maximumCriticalDamage != 0  && maximumCriticalDamage < finalDamage)
        {
            finalDamage = maximumCriticalDamage;
        }

        if (finalDamage <= 0)
        {
            finalDamage = 1;
        }
        
        #endregion
        
        return new CalculationResult(finalDamage, isCritical, isSoftDamage);
    }

    private static int GetMonsterBaseDamage(IBattleEntityDump attacker, IBattleEntityDump defender)
    {
        if (!attacker.IsMonster())
        {
            return 0;
        }

        int monsterLevel = attacker.Level;
        int multiplier = monsterLevel switch
        {
            < 30 => 0,
            <= 50 => 1,
            < 60 => 2,
            < 65 => 3,
            < 70 => 4,
            _ => 5
        };

        double damageReduction = defender is PlayerBattleEntityDump playerBattleEntityDump ? playerBattleEntityDump.MinimalDamageReduction : 1;

        return (int)(monsterLevel * multiplier * damageReduction);
    }

    private static int Penalties(this IBattleEntityDump attacker, IBattleEntityDump defender, int damage)
    {
        if (attacker.AttackType != AttackType.Ranged)
        {
            return damage;
        }
        
        int distance = attacker.Position.GetDistance(defender.Position);

        bool hasRangePenalty = !attacker.HasBCard(BCardType.GuarantedDodgeRangedAttack, (byte)AdditionalTypes.GuarantedDodgeRangedAttack.NoPenalty);
        bool increaseDamageRangeDistance = attacker.HasBCard(BCardType.GuarantedDodgeRangedAttack, (byte)AdditionalTypes.GuarantedDodgeRangedAttack.DistanceDamageIncreasing);
        bool hasIncreaseDamageByDistance = attacker.HasBCard(BCardType.HideBarrelSkill, (byte)AdditionalTypes.HideBarrelSkill.IncreasesDamageByDistance);
        double increaseDamageByDistance = attacker.GetBCardInformation(BCardType.HideBarrelSkill, (byte)AdditionalTypes.HideBarrelSkill.IncreasesDamageByDistance).firstData * 0.01;

        int returnDamage = damage;
        returnDamage = distance switch
        {
            <= 2 when hasRangePenalty => (int)(returnDamage * 0.7),
            > 2 when increaseDamageRangeDistance => (int)(returnDamage * (0.95 + 0.05 * distance)),
            > 2 when hasIncreaseDamageByDistance => (int)(returnDamage * (0.95 + increaseDamageByDistance * distance)),
            _ => returnDamage
        };

        return returnDamage;
    }
}