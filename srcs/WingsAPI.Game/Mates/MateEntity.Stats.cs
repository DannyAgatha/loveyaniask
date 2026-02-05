using WingsEmu.Game._enum;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Items;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;

namespace WingsEmu.Game.Mates;

public partial class MateEntity
{
    private int _criticalChance;
    private int _criticalDamage;
    private int _damageMax;
    private int _damageMin;
    private short _hitRate;
    private short _magicDefense;
    private int _maxHp;
    private int _maxMp;
    private short _meleeDefense;
    private short _meleeDodge;
    private short _rangedDefense;
    private short _rangedDodge;
    private byte _speed;

    public void RefreshStatistics()
    {
        _maxHp = _algorithm.GetBasicHp((short)MonsterRaceType, Level, MeleeHpFactor, CleanHp, false) + _algorithm.GetStarsHpMp(Stars, HeroLevel);
        _maxMp = _algorithm.GetBasicMp((short)MonsterRaceType, Level, MagicMpFactor, CleanMp, false) + _algorithm.GetStarsHpMp(Stars, HeroLevel);

        _hitRate = (short)(_algorithm.GetHitrate((short)MonsterRaceType, AttackType, WeaponLevel, BaseLevel,
            RangeDodgeFactor, CleanHitRate, false, Level, MateType) + _algorithm.GetStarsHitRate(Stars, HeroLevel));
        _damageMin = _algorithm.GetAttack(true, (short)MonsterRaceType, AttackType, WeaponLevel, WinfoValue, BaseLevel,
            this.GetModifier(), CleanDamageMin, false, Level, MateType) + _algorithm.GetStarsAttackPower(Stars, HeroLevel);
        _damageMax = _algorithm.GetAttack(false, (short)MonsterRaceType, AttackType, WeaponLevel, WinfoValue, BaseLevel,
            this.GetModifier(), CleanDamageMax, false, Level, MateType) + _algorithm.GetStarsAttackPower(Stars, HeroLevel);

        _criticalChance = MateType == MateType.Partner ? 0 : BaseCriticalChance;
        _criticalDamage = MateType == MateType.Partner ? 0 : BaseCriticalRate;

        _meleeDefense = (short)(_algorithm.GetDefense((short)MonsterRaceType, AttackType.Melee, ArmorLevel, BaseLevel,
            MeleeHpFactor, CleanMeleeDefence, false, Level, MateType) + _algorithm.GetStarsAllDefences(Stars, HeroLevel));
        _rangedDefense = (short)(_algorithm.GetDefense((short)MonsterRaceType, AttackType.Ranged, ArmorLevel, BaseLevel,
            RangeDodgeFactor, CleanRangeDefence, false, Level, MateType) + _algorithm.GetStarsAllDefences(Stars, HeroLevel));
        _magicDefense = (short)(_algorithm.GetDefense((short)MonsterRaceType, AttackType.Magical, ArmorLevel, BaseLevel,
            MagicMpFactor, CleanMagicDefence, false, Level, MateType) + _algorithm.GetStarsAllDefences(Stars, HeroLevel));
        _meleeDodge = (short)(_algorithm.GetDodge((short)MonsterRaceType, ArmorLevel, BaseLevel,
            RangeDodgeFactor, CleanDodge, false, Level, MateType) + _algorithm.GetStarsAllDefences(Stars, HeroLevel));
        _rangedDodge = (short)(_algorithm.GetDodge((short)MonsterRaceType, ArmorLevel, BaseLevel,
            RangeDodgeFactor, CleanDodge, false, Level, MateType) + _algorithm.GetStarsAllDefences(Stars, HeroLevel));

        StatisticsComponent.RefreshMateStatistics(this);
    }

    private short GetMoreStats(StatisticType type, short baseStats) => (short)(this.FindMoreStats(type) + baseStats);

    private int GetMateDamage(int baseDamage, bool isMin)
    {
        GameItemInstance weapon = Weapon;
        
        if (IsUsingSp)
        {
            baseDamage += AttackType switch
            {
                AttackType.Melee => this.FindMoreStats(StatisticType.ATTACK_MELEE),
                AttackType.Magical => this.FindMoreStats(StatisticType.ATTACK_MAGIC),
                AttackType.Ranged => this.FindMoreStats(StatisticType.ATTACK_RANGED),
                _ => 0
            };
        }
        
        if (weapon == null)
        {
            return baseDamage;
        }

        int toAdd;
        if (isMin)
        {
            toAdd = weapon.DamageMinimum + weapon.GameItem.DamageMinimum;
        }
        else
        {
            toAdd = weapon.DamageMaximum + weapon.GameItem.DamageMaximum;
        }

        return toAdd + baseDamage;
    }

    private int GetMateCritical(int baseCritical, bool isChance)
    {
        if (MateType == MateType.Pet)
        {
            return baseCritical;
        }

        GameItemInstance weapon = Weapon;
        if (weapon == null)
        {
            return baseCritical;
        }

        int toAdd;
        if (isChance)
        {
            toAdd = weapon.GameItem.CriticalLuckRate;
        }
        else
        {
            toAdd = weapon.GameItem.CriticalRate;
        }

        return toAdd + baseCritical;
    }

    private int GetMateHitRate(short hitRate)
    {
        int stats = AttackType switch
        {
            AttackType.Melee => this.FindMoreStats(StatisticType.HITRATE_MELEE),
            AttackType.Ranged => this.FindMoreStats(StatisticType.HITRATE_RANGED),
            AttackType.Magical => this.FindMoreStats(StatisticType.HITRATE_MAGIC),
            _ => 0
        };

        return stats + hitRate;
    }
}