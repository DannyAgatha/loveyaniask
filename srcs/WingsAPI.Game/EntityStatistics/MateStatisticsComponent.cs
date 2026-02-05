using System.Collections.Generic;
using System.Linq;
using WingsAPI.Data.Families;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Mates;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;

namespace WingsEmu.Game.EntityStatistics;

public class MateStatisticsComponent : IMateStatisticsComponent
{
    private readonly Dictionary<Statistics, int> _stats = new();
    private IMateEntity _mateEntity;

    public MateStatisticsComponent(IMateEntity mateEntity) => _mateEntity = mateEntity;

    public int MinDamage => _mateEntity?.DamagesMinimum + _stats.GetValueOrDefault(Statistics.MIN_DAMAGE) ?? 0;
    public int MaxDamage => _mateEntity?.DamagesMaximum + _stats.GetValueOrDefault(Statistics.MAX_DAMAGE) ?? 0;
    public int HitRate => _mateEntity?.HitRate + _stats.GetValueOrDefault(Statistics.MIN_DAMAGE) ?? 0;
    public int CriticalChance => _mateEntity?.HitCriticalChance + _stats.GetValueOrDefault(Statistics.CRITICAL_CHANCE) ?? 0;
    public int CriticalDamage => _mateEntity?.HitCriticalDamage + _stats.GetValueOrDefault(Statistics.CRITICAL_DAMAGE) ?? 0;
    public int MeleeDefense => _mateEntity?.CloseDefence + _stats.GetValueOrDefault(Statistics.MELEE_DEFENSE) ?? 0;
    public int RangeDefense => _mateEntity?.DistanceDefence + _stats.GetValueOrDefault(Statistics.RANGE_DEFENSE) ?? 0;
    public int MagicDefense => _mateEntity?.MagicDefence + _stats.GetValueOrDefault(Statistics.MAGIC_DEFENSE) ?? 0;
    public int MeleeDodge => _mateEntity?.DefenceDodge + _stats.GetValueOrDefault(Statistics.MELEE_DODGE) ?? 0;
    public int RangeDodge => _mateEntity?.DistanceDodge + _stats.GetValueOrDefault(Statistics.RANGE_DODGE) ?? 0;
    public int FireResistance => _mateEntity?.FireResistance + _stats.GetValueOrDefault(Statistics.FIRE_RESISTANCE) ?? 0;
    public int WaterResistance => _mateEntity?.WaterResistance + _stats.GetValueOrDefault(Statistics.WATER_RESISTANCE) ?? 0;
    public int LightResistance => _mateEntity?.LightResistance + _stats.GetValueOrDefault(Statistics.LIGHT_RESISTANCE) ?? 0;
    public int ShadowResistance => _mateEntity?.DarkResistance + _stats.GetValueOrDefault(Statistics.SHADOW_RESISTANCE) ?? 0;

    public void RefreshMateStatistics(IMateEntity mateEntity)
    {
        _mateEntity = mateEntity;
        if (_mateEntity.MateType == MateType.Pet)
        {
            return;
        }

        _stats.Clear();
        int minDamage = 0;
        int maxDamage = 0;
        int hitRate = 0;
        int criticalChance = 0;
        int criticalDamage = 0;
        int meleeDefense = 0;
        int rangeDefense = 0;
        int magicDefense = 0;
        int meleeDodge = 0;
        int rangeDodge = 0;
        int fireResistance = 0;
        int waterResistance = 0;
        int lightResistance = 0;
        int shadowResistance = 0;

        AttackType attackType = _mateEntity.AttackType;

        byte playerLevel = _mateEntity.Level;

        IReadOnlyList<BCardDTO> bCards = _mateEntity.BCardComponent.GetAllBCards();
        IEnumerable<BCardDTO> minMaxDamageBCards = bCards.Where(x => x.Type == (short)BCardType.AttackPower);
        IEnumerable<BCardDTO> hitRateBCards = bCards.Where(x => x.Type == (short)BCardType.Target);
        IEnumerable<BCardDTO> criticalBCards = bCards.Where(x => x.Type == (short)BCardType.Critical);
        IEnumerable<BCardDTO> defenseBCards = bCards.Where(x => x.Type == (short)BCardType.Defence);
        IEnumerable<BCardDTO> dodgeBCards = bCards.Where(x => x.Type == (short)BCardType.DodgeAndDefencePercent);
        IEnumerable<BCardDTO> resistanceBCards = bCards.Where(x => x.Type == (short)BCardType.ElementResistance);

        foreach (BCardDTO bCard in minMaxDamageBCards)
        {
            int firstData = bCard.FirstDataValue(playerLevel);

            switch ((AdditionalTypes.AttackPower)bCard.SubType)
            {
                case AdditionalTypes.AttackPower.AllAttacksIncreased:
                    minDamage += firstData;
                    maxDamage += firstData;
                    break;
                case AdditionalTypes.AttackPower.AllAttacksDecreased:
                    minDamage -= firstData;
                    maxDamage -= firstData;
                    break;
                case AdditionalTypes.AttackPower.MeleeAttacksIncreased:
                    if (attackType != AttackType.Melee)
                    {
                        break;
                    }

                    minDamage += firstData;
                    maxDamage += firstData;
                    break;
                case AdditionalTypes.AttackPower.MeleeAttacksDecreased:
                    if (attackType != AttackType.Melee)
                    {
                        break;
                    }

                    minDamage -= firstData;
                    maxDamage -= firstData;
                    break;
                case AdditionalTypes.AttackPower.RangedAttacksIncreased:
                    if (attackType != AttackType.Ranged)
                    {
                        break;
                    }

                    minDamage += firstData;
                    maxDamage += firstData;
                    break;
                case AdditionalTypes.AttackPower.RangedAttacksDecreased:
                    if (attackType != AttackType.Ranged)
                    {
                        break;
                    }

                    minDamage -= firstData;
                    maxDamage -= firstData;
                    break;
                case AdditionalTypes.AttackPower.MagicalAttacksIncreased:
                    if (attackType != AttackType.Magical)
                    {
                        break;
                    }

                    minDamage += firstData;
                    maxDamage += firstData;
                    break;
                case AdditionalTypes.AttackPower.MagicalAttacksDecreased:
                    if (attackType != AttackType.Magical)
                    {
                        break;
                    }

                    minDamage -= firstData;
                    maxDamage -= firstData;
                    break;
            }
        }

        foreach (BCardDTO bCard in hitRateBCards)
        {
            int firstData = bCard.FirstDataValue(playerLevel);
            switch ((AdditionalTypes.Target)bCard.SubType)
            {
                case AdditionalTypes.Target.AllHitRateIncreased:
                    hitRate += firstData;
                    break;
                case AdditionalTypes.Target.AllHitRateDecreased:
                    hitRate -= firstData;
                    break;
                case AdditionalTypes.Target.MeleeHitRateIncreased:
                    if (attackType != AttackType.Melee)
                    {
                        break;
                    }

                    hitRate += firstData;
                    break;
                case AdditionalTypes.Target.MeleeHitRateDecreased:
                    if (attackType != AttackType.Melee)
                    {
                        break;
                    }

                    hitRate -= firstData;
                    break;
                case AdditionalTypes.Target.RangedHitRateIncreased:
                    if (attackType != AttackType.Ranged)
                    {
                        break;
                    }

                    hitRate += firstData;
                    break;
                case AdditionalTypes.Target.RangedHitRateDecreased:
                    if (attackType != AttackType.Ranged)
                    {
                        break;
                    }

                    hitRate -= firstData;
                    break;
                case AdditionalTypes.Target.MagicalConcentrationIncreased:
                    if (attackType != AttackType.Magical)
                    {
                        break;
                    }

                    hitRate += firstData;
                    break;
                case AdditionalTypes.Target.MagicalConcentrationDecreased:
                    if (attackType != AttackType.Magical)
                    {
                        break;
                    }

                    hitRate -= firstData;
                    break;
            }
            
            switch ((AdditionalTypes.IncreaseAllDamage)bCard.SubType)
            {
                case AdditionalTypes.IncreaseAllDamage.ConcentrationIncrease:
                    if (attackType != AttackType.Magical)
                    {
                        break;
                    }

                    hitRate += firstData;
                    break;
                case AdditionalTypes.IncreaseAllDamage.ConcentrationDecrease:
                    if (attackType != AttackType.Magical)
                    {
                        break;
                    }

                    hitRate -= firstData;
                    break;
            }
        }

        foreach (BCardDTO bCard in criticalBCards)
        {
            int firstData = bCard.FirstDataValue(playerLevel);

            switch ((AdditionalTypes.Critical)bCard.SubType)
            {
                case AdditionalTypes.Critical.InflictingIncreased:
                    if (attackType == AttackType.Magical)
                    {
                        break;
                    }

                    criticalChance += firstData;
                    break;
                case AdditionalTypes.Critical.InflictingReduced:
                    if (attackType == AttackType.Magical)
                    {
                        break;
                    }

                    criticalChance -= firstData;
                    break;
                case AdditionalTypes.Critical.DamageIncreased:
                    if (attackType == AttackType.Magical)
                    {
                        break;
                    }

                    criticalDamage += firstData;
                    break;
                case AdditionalTypes.Critical.DamageIncreasedInflictingReduced:
                    if (attackType == AttackType.Magical)
                    {
                        break;
                    }

                    criticalDamage -= firstData;
                    break;
            }
        }

        foreach (BCardDTO bCard in defenseBCards)
        {
            int firstData = bCard.FirstDataValue(playerLevel);

            switch ((AdditionalTypes.Defence)bCard.SubType)
            {
                case AdditionalTypes.Defence.AllIncreased:
                    meleeDefense += firstData;
                    rangeDefense += firstData;
                    magicDefense += firstData;
                    break;
                case AdditionalTypes.Defence.AllDecreased:
                    meleeDefense -= firstData;
                    rangeDefense -= firstData;
                    magicDefense -= firstData;
                    break;
                case AdditionalTypes.Defence.MeleeIncreased:
                    meleeDefense += firstData;
                    break;
                case AdditionalTypes.Defence.MeleeDecreased:
                    meleeDefense -= firstData;
                    break;
                case AdditionalTypes.Defence.RangedIncreased:
                    rangeDefense += firstData;
                    break;
                case AdditionalTypes.Defence.RangedDecreased:
                    rangeDefense -= firstData;
                    break;
                case AdditionalTypes.Defence.MagicalIncreased:
                    magicDefense += firstData;
                    break;
                case AdditionalTypes.Defence.MagicalDecreased:
                    magicDefense -= firstData;
                    break;
            }
        }

        foreach (BCardDTO bCard in dodgeBCards)
        {
            int firstData = bCard.FirstDataValue(playerLevel);

            switch ((AdditionalTypes.DodgeAndDefencePercent)bCard.SubType)
            {
                case AdditionalTypes.DodgeAndDefencePercent.DodgeIncreased:
                    meleeDodge += firstData;
                    rangeDodge += firstData;
                    break;
                case AdditionalTypes.DodgeAndDefencePercent.DodgeDecreased:
                    meleeDodge -= firstData;
                    rangeDodge -= firstData;
                    break;
                case AdditionalTypes.DodgeAndDefencePercent.DodgingMeleeIncreased:
                    meleeDodge += firstData;
                    break;
                case AdditionalTypes.DodgeAndDefencePercent.DodgingMeleeDecreased:
                    meleeDodge -= firstData;
                    break;
                case AdditionalTypes.DodgeAndDefencePercent.DodgingRangedIncreased:
                    rangeDodge += firstData;
                    break;
                case AdditionalTypes.DodgeAndDefencePercent.DodgingRangedDecreased:
                    rangeDodge -= firstData;
                    break;
            }
        }

        foreach (BCardDTO bCard in resistanceBCards)
        {
            int firstData = bCard.FirstDataValue(playerLevel);

            switch ((AdditionalTypes.ElementResistance)bCard.SubType)
            {
                case AdditionalTypes.ElementResistance.AllIncreased:
                    fireResistance += firstData;
                    waterResistance += firstData;
                    lightResistance += firstData;
                    shadowResistance += firstData;
                    break;
                case AdditionalTypes.ElementResistance.AllDecreased:
                    fireResistance -= firstData;
                    waterResistance -= firstData;
                    lightResistance -= firstData;
                    shadowResistance -= firstData;
                    break;
                case AdditionalTypes.ElementResistance.FireIncreased:
                    fireResistance += firstData;
                    break;
                case AdditionalTypes.ElementResistance.FireDecreased:
                    fireResistance -= firstData;
                    break;
                case AdditionalTypes.ElementResistance.WaterIncreased:
                    waterResistance += firstData;
                    break;
                case AdditionalTypes.ElementResistance.WaterDecreased:
                    waterResistance -= firstData;
                    break;
                case AdditionalTypes.ElementResistance.LightIncreased:
                    lightResistance += firstData;
                    break;
                case AdditionalTypes.ElementResistance.LightDecreased:
                    lightResistance -= firstData;
                    break;
                case AdditionalTypes.ElementResistance.DarkIncreased:
                    shadowResistance += firstData;
                    break;
                case AdditionalTypes.ElementResistance.DarkDecreased:
                    shadowResistance -= firstData;
                    break;
            }
        }

        fireResistance += _mateEntity.Owner?.Family?.UpgradeValues?.GetOrDefault(FamilyUpgradeType.FIRE_RESISTANCE).Item1 ?? 0;
        waterResistance += _mateEntity.Owner?.Family?.UpgradeValues?.GetOrDefault(FamilyUpgradeType.WATER_RESISTANCE).Item1 ?? 0;
        lightResistance += _mateEntity.Owner?.Family?.UpgradeValues?.GetOrDefault(FamilyUpgradeType.LIGHT_RESISTANCE).Item1 ?? 0;
        shadowResistance += _mateEntity.Owner?.Family?.UpgradeValues?.GetOrDefault(FamilyUpgradeType.DARK_RESISTANCE).Item1 ?? 0;

        _stats[Statistics.MIN_DAMAGE] = minDamage;
        _stats[Statistics.MAX_DAMAGE] = maxDamage;
        _stats[Statistics.HITRATE] = hitRate;
        _stats[Statistics.CRITICAL_CHANCE] = criticalChance;
        _stats[Statistics.CRITICAL_DAMAGE] = criticalDamage;
        _stats[Statistics.MELEE_DEFENSE] = meleeDefense;
        _stats[Statistics.RANGE_DEFENSE] = rangeDefense;
        _stats[Statistics.MAGIC_DEFENSE] = magicDefense;
        _stats[Statistics.MELEE_DODGE] = meleeDodge;
        _stats[Statistics.RANGE_DODGE] = rangeDodge;
        _stats[Statistics.FIRE_RESISTANCE] = fireResistance;
        _stats[Statistics.WATER_RESISTANCE] = waterResistance;
        _stats[Statistics.LIGHT_RESISTANCE] = lightResistance;
        _stats[Statistics.SHADOW_RESISTANCE] = shadowResistance;
    }
}