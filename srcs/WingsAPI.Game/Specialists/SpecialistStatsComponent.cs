using WingsAPI.Packets.Enums.Shells;
using WingsEmu.Game._enum;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Specialists;

public class SpecialistStatsComponent : ISpecialistStatsComponent
{
    private readonly IPlayerEntity _playerEntity;

    public SpecialistStatsComponent(IPlayerEntity playerEntity) => _playerEntity = playerEntity;

    private GameItemInstance Specialist => _playerEntity.Specialist;

    public int GetSlHit(byte activeSlot)
    {
        if (Specialist == null)
        {
            return 0;
        }

        int slHit = Specialist.SlPoint(Specialist.SlDamage, SpecialistPointsType.ATTACK);
        slHit += _playerEntity.GetMaxWeaponShellValue(ShellEffectType.SLDamage) +
            _playerEntity.GetMaxWeaponShellValue(ShellEffectType.SLGlobal) +
            _playerEntity.BCardComponent.GetAllBCardsInformation(BCardType.IncreaseSpPoints,
                (byte)AdditionalTypes.IncreaseSpPoints.SpCardAttackPointIncrease, _playerEntity.Level).firstData +
            _playerEntity.BCardComponent.GetAllBCardsInformation(BCardType.SPCardUpgrade,
                (byte)AdditionalTypes.SPCardUpgrade.IncreaseAllSkillPoints, _playerEntity.Level).firstData;

        slHit = slHit > GetSlMax() ? GetSlMax() : slHit;

        return slHit;
    }
    
    private int GetSlMax()
    {
        return Specialist.Upgrade switch
        {
            <= 15 => 100,
            <= 16 => 105,
            <= 17 => 110,
            <= 18 => 115,
            <= 19 => 120,
            _ => 120
        };
    }

    public int GetSlDefense(byte activeSlot)
    {
        if (Specialist == null)
        {
            return 0;
        }

        int slDefence = Specialist.SlPoint(Specialist.SlDefence, SpecialistPointsType.DEFENCE);

        slDefence += _playerEntity.GetMaxWeaponShellValue(ShellEffectType.SLDefence) +
            _playerEntity.GetMaxWeaponShellValue(ShellEffectType.SLGlobal) +
            _playerEntity.BCardComponent.GetAllBCardsInformation(BCardType.IncreaseSpPoints,
                (byte)AdditionalTypes.IncreaseSpPoints.SpCardDefensePointIncrease, _playerEntity.Level).firstData +
            _playerEntity.BCardComponent.GetAllBCardsInformation(BCardType.SPCardUpgrade,
                (byte)AdditionalTypes.SPCardUpgrade.IncreaseAllSkillPoints, _playerEntity.Level).firstData;

        slDefence = slDefence > GetSlMax() ? GetSlMax() : slDefence;

        return slDefence;
    }

    public int GetSlElement(byte activeSlot)
    {
        if (Specialist == null)
        {
            return 0;
        }

        int slElement = Specialist.SlPoint(Specialist.SlElement, SpecialistPointsType.ELEMENT);

        slElement += _playerEntity.GetMaxWeaponShellValue(ShellEffectType.SLElement) +
            _playerEntity.GetMaxWeaponShellValue(ShellEffectType.SLGlobal) +
            _playerEntity.BCardComponent.GetAllBCardsInformation(BCardType.IncreaseSpPoints,
                (byte)AdditionalTypes.IncreaseSpPoints.SpCardElementPointIncrease, _playerEntity.Level).firstData +
            _playerEntity.BCardComponent.GetAllBCardsInformation(BCardType.SPCardUpgrade,
                (byte)AdditionalTypes.SPCardUpgrade.IncreaseAllSkillPoints, _playerEntity.Level).firstData;

        slElement = slElement > GetSlMax() ? GetSlMax() : slElement;

        return slElement;
    }

    public int GetSlHp(byte activeSlot)
    {
        if (Specialist == null)
        {
            return 0;
        }

        int slHp = Specialist.SlPoint(Specialist.SlHp, SpecialistPointsType.HPMP);

        slHp += _playerEntity.GetMaxWeaponShellValue(ShellEffectType.SLHP) +
            _playerEntity.GetMaxWeaponShellValue(ShellEffectType.SLGlobal) +
            _playerEntity.BCardComponent.GetAllBCardsInformation(BCardType.IncreaseSpPoints,
                (byte)AdditionalTypes.IncreaseSpPoints.SpCardHpMpPointIncrease, _playerEntity.Level).firstData +
            _playerEntity.BCardComponent.GetAllBCardsInformation(BCardType.SPCardUpgrade,
                (byte)AdditionalTypes.SPCardUpgrade.IncreaseAllSkillPoints, _playerEntity.Level).firstData;

        slHp = slHp > GetSlMax() ? GetSlMax() : slHp;

        return slHp;
    }

    public int DamageMinimum { get; set; }
    public int DamageMaximum { get; set; }
    public int HitRate { get; set; }
    public int CriticalLuckRate { get; set; }
    public int CriticalRate { get; set; }
    public int DefenceDodge { get; set; }
    public int DistanceDefenceDodge { get; set; }
    public int ElementRate { get; set; }
    public int DarkResistance { get; set; }
    public int LightResistance { get; set; }
    public int FireResistance { get; set; }
    public int WaterResistance { get; set; }
    public int CriticalDodge { get; set; }
    public int CloseDefence { get; set; }
    public int DistanceDefence { get; set; }
    public int MagicDefence { get; set; }
    public int Hp { get; set; }
    public int Mp { get; set; }
    public int SpDamage => Specialist?.SpDamage ?? 0;
    public int SpDefence => Specialist?.SpDefence ?? 0;
    public int SpHP => Specialist?.SpHP ?? 0;
    public int SpElement => Specialist?.SpElement ?? 0;
    public int SpDark => Specialist?.SpDark ?? 0;
    public int SpFire => Specialist?.SpFire ?? 0;
    public int SpWater => Specialist?.SpWater ?? 0;
    public int SpLight => Specialist?.SpLight ?? 0;
    public int SpAttackPassive => Specialist?.SpAttackPassive ?? 0;
    public int SpCritProbabilityPassive => Specialist?.SpCritProbabilityPassive ?? 0;
    public int SpPercentBonusPassive => Specialist?.SpPercentBonusPassive ?? 0;
    public int SpMeleeDefenceBonusPassive => Specialist?.SpMeleeDefenceBonusPassive ?? 0;
    public int SpRangedDefenceBonusPassive => Specialist?.SpRangedDefenceBonusPassive ?? 0;
    public int SpMagicalDefenceBonusPassive => Specialist?.SpMagicalDefenceBonusPassive ?? 0;
    public int SlotStatus => Specialist?.SlotStatus ?? 0;
    public int SlotStatus2 => Specialist?.Slot2Status ?? 0;
    public int SlotStatus3 => Specialist?.Slot3Status ?? 0;

    public void RefreshSlStats(byte activeSlot)
    {
        DamageMinimum = 0;
        DamageMaximum = 0;
        HitRate = 0;
        CriticalLuckRate = 0;
        CriticalRate = 0;
        DefenceDodge = 0;
        DistanceDefenceDodge = 0;
        ElementRate = 0;
        DarkResistance = 0;
        LightResistance = 0;
        FireResistance = 0;
        WaterResistance = 0;
        CriticalDodge = 0;
        CloseDefence = 0;
        DistanceDefence = 0;
        MagicDefence = 0;
        Hp = 0;
        Mp = 0;

        if (Specialist == null)
        {
            return;
        }

        int slHit = GetSlHit(activeSlot);
        int slDefence = GetSlDefense(activeSlot);
        int slElement = GetSlElement(activeSlot);
        int slHp = GetSlHp(activeSlot);


        #region slHit

        if (slHit >= 1)
        {
            DamageMinimum += 5;
            DamageMaximum += 5;
        }

        if (slHit >= 10)
        {
            HitRate += 10;
        }

        if (slHit >= 20)
        {
            CriticalLuckRate += 2;
        }

        if (slHit >= 30)
        {
            DamageMinimum += 5;
            DamageMaximum += 5;
            HitRate += 10;
        }

        if (slHit >= 40)
        {
            CriticalRate += 10;
        }

        if (slHit >= 50)
        {
            Hp += 200;
            Mp += 200;
        }

        if (slHit >= 60)
        {
            HitRate += 15;
        }

        if (slHit >= 70)
        {
            HitRate += 15;
            DamageMinimum += 5;
            DamageMaximum += 5;
        }

        if (slHit >= 80)
        {
            CriticalLuckRate += 3;
        }

        if (slHit >= 90)
        {
            CriticalRate += 20;
        }

        if (slHit >= 100)
        {
            CriticalLuckRate += 3;
            CriticalRate += 20;
            Hp += 200;
            Mp += 200;
            DamageMinimum += 5;
            DamageMaximum += 5;
            HitRate += 20;
        }
        
        if (slHit >= 110)
        {
            HitRate += 20;
            DamageMinimum += 10;
            DamageMaximum += 10;
            CriticalRate += 30;
        }

        if (slHit >= 115)
        {
            Hp += 200;
            Mp += 200;
        }

        if (slHit >= 120)
        {
            Hp += 200;
            Mp += 200;
            CriticalLuckRate += 2;
            CriticalRate += 10;
            DamageMinimum += 15;
            DamageMaximum += 15;
        }

        #endregion

        #region slDefence

        if (slDefence >= 10)
        {
            DefenceDodge += 5;
            DistanceDefenceDodge += 5;
        }

        if (slDefence >= 20)
        {
            CloseDefence += 1;
            DistanceDefence += 1;
            MagicDefence += 1;
            CriticalDodge += 2;
        }

        if (slDefence >= 30)
        {
            Hp += 100;
        }

        if (slDefence >= 40)
        {
            CloseDefence += 1;
            DistanceDefence += 1;
            MagicDefence += 1;
            CriticalDodge += 2;
        }

        if (slDefence >= 50)
        {
            DefenceDodge += 5;
            DistanceDefenceDodge += 5;
        }

        if (slDefence >= 60)
        {
            Hp += 200;
        }

        if (slDefence >= 70)
        {
            CriticalDodge += 3;
        }

        if (slDefence >= 75)
        {
            FireResistance += 2;
            WaterResistance += 2;
            LightResistance += 2;
            DarkResistance += 2;
            CloseDefence += 1;
            DistanceDefence += 1;
            MagicDefence += 1;
        }

        if (slDefence >= 80)
        {
            DefenceDodge += 10;
            DistanceDefenceDodge += 10;
            CriticalDodge += 3;
        }

        if (slDefence >= 90)
        {
            FireResistance += 3;
            WaterResistance += 3;
            LightResistance += 3;
            DarkResistance += 3;
            CloseDefence += 1;
            DistanceDefence += 1;
            MagicDefence += 1;
        }

        if (slDefence >= 95)
        {
            Hp += 300;
        }

        if (slDefence >= 100)
        {
            DefenceDodge += 20;
            DistanceDefenceDodge += 20;
            FireResistance += 5;
            WaterResistance += 5;
            LightResistance += 5;
            DarkResistance += 5;
            CloseDefence += 1;
            DistanceDefence += 1;
            MagicDefence += 1;
        }
        
        if (slDefence >= 110)
        {
            DefenceDodge += 10;
            DistanceDefenceDodge += 10;
            CriticalDodge += 4;
        }

        if (slDefence >= 115)
        {
            Hp += 400;
        }

        if (slDefence >= 120)
        {
            CriticalDodge += 20;
            FireResistance += 5;
            WaterResistance += 5;
            LightResistance += 5;
            DarkResistance += 5;
        }

        #endregion

        #region slHp

        if (slHp >= 5)
        {
            DamageMinimum += 5;
            DamageMaximum += 5;
        }

        if (slHp >= 10)
        {
            DamageMinimum += 5;
            DamageMaximum += 5;
        }

        if (slHp >= 15)
        {
            DamageMinimum += 5;
            DamageMaximum += 5;
        }

        if (slHp >= 20)
        {
            DamageMinimum += 5;
            DamageMaximum += 5;
            CloseDefence += 10;
            DistanceDefence += 10;
            MagicDefence += 10;
        }

        if (slHp >= 25)
        {
            DamageMinimum += 5;
            DamageMaximum += 5;
        }

        if (slHp >= 30)
        {
            DamageMinimum += 5;
            DamageMaximum += 5;
        }

        if (slHp >= 35)
        {
            DamageMinimum += 5;
            DamageMaximum += 5;
        }

        if (slHp >= 40)
        {
            DamageMinimum += 5;
            DamageMaximum += 5;
            CloseDefence += 15;
            DistanceDefence += 15;
            MagicDefence += 15;
        }

        if (slHp >= 45)
        {
            DamageMinimum += 10;
            DamageMaximum += 10;
        }

        if (slHp >= 50)
        {
            DamageMinimum += 10;
            DamageMaximum += 10;
            FireResistance += 2;
            WaterResistance += 2;
            LightResistance += 2;
            DarkResistance += 2;
        }

        if (slHp >= 55)
        {
            DamageMinimum += 10;
            DamageMaximum += 10;
        }

        if (slHp >= 60)
        {
            DamageMinimum += 10;
            DamageMaximum += 10;
        }

        if (slHp >= 65)
        {
            DamageMinimum += 10;
            DamageMaximum += 10;
        }

        if (slHp >= 70)
        {
            DamageMinimum += 10;
            DamageMaximum += 10;
            CloseDefence += 20;
            DistanceDefence += 20;
            MagicDefence += 20;
        }

        if (slHp >= 75)
        {
            DamageMinimum += 15;
            DamageMaximum += 15;
        }

        if (slHp >= 80)
        {
            DamageMinimum += 15;
            DamageMaximum += 15;
        }

        if (slHp >= 85)
        {
            DamageMinimum += 15;
            DamageMaximum += 15;
            CriticalDodge += 1;
        }

        if (slHp >= 86)
        {
            CriticalDodge += 1;
        }

        if (slHp >= 87)
        {
            CriticalDodge += 1;
        }

        if (slHp >= 88)
        {
            CriticalDodge += 1;
        }

        if (slHp >= 90)
        {
            DamageMinimum += 15;
            DamageMaximum += 15;
            CloseDefence += 25;
            DistanceDefence += 25;
            MagicDefence += 25;
        }

        if (slHp >= 91)
        {
            DefenceDodge += 2;
            DistanceDefenceDodge += 2;
        }

        if (slHp >= 92)
        {
            DefenceDodge += 2;
            DistanceDefenceDodge += 2;
        }

        if (slHp >= 93)
        {
            DefenceDodge += 2;
            DistanceDefenceDodge += 2;
        }

        if (slHp >= 94)
        {
            DefenceDodge += 2;
            DistanceDefenceDodge += 2;
        }

        if (slHp >= 95)
        {
            DamageMinimum += 20;
            DamageMaximum += 20;
            DefenceDodge += 2;
            DistanceDefenceDodge += 2;
        }

        if (slHp >= 96)
        {
            DefenceDodge += 2;
            DistanceDefenceDodge += 2;
        }

        if (slHp >= 97)
        {
            DefenceDodge += 2;
            DistanceDefenceDodge += 2;
        }

        if (slHp >= 98)
        {
            DefenceDodge += 2;
            DistanceDefenceDodge += 2;
        }

        if (slHp >= 99)
        {
            DefenceDodge += 2;
            DistanceDefenceDodge += 2;
        }

        if (slHp >= 100)
        {
            FireResistance += 3;
            WaterResistance += 3;
            LightResistance += 3;
            DarkResistance += 3;
            CloseDefence += 30;
            DistanceDefence += 30;
            MagicDefence += 30;
            DamageMinimum += 20;
            DamageMaximum += 20;
            DefenceDodge += 2;
            DistanceDefenceDodge += 2;
            CriticalDodge += 1;
        }
        
        if (slHp >= 110)
        {
            DamageMinimum += 30;
            DamageMaximum += 30;
            DefenceDodge += 4;
            DistanceDefenceDodge += 4;
        }

        if (slHp >= 115)
        {
            CloseDefence += 35;
            DistanceDefence += 35;
            MagicDefence += 35;
        }

        if (slHp >= 120)
        {
            FireResistance += 2;
            WaterResistance += 2;
            LightResistance += 2;
            DarkResistance += 2;
            CriticalDodge += 2;
        }

        #endregion

        #region slElement

        if (slElement >= 1)
        {
            ElementRate += 2;
        }

        if (slElement >= 10)
        {
            Mp += 100;
        }

        if (slElement >= 20)
        {
            MagicDefence += 5;
        }

        if (slElement >= 30)
        {
            FireResistance += 2;
            WaterResistance += 2;
            LightResistance += 2;
            DarkResistance += 2;
            ElementRate += 2;
        }

        if (slElement >= 40)
        {
            Mp += 100;
        }

        if (slElement >= 50)
        {
            MagicDefence += 5;
        }

        if (slElement >= 60)
        {
            FireResistance += 3;
            WaterResistance += 3;
            LightResistance += 3;
            DarkResistance += 3;
            ElementRate += 2;
        }

        if (slElement >= 70)
        {
            Mp += 100;
        }

        if (slElement >= 80)
        {
            MagicDefence += 5;
        }

        if (slElement >= 90)
        {
            FireResistance += 4;
            WaterResistance += 4;
            LightResistance += 4;
            DarkResistance += 4;
            ElementRate += 2;
        }

        if (slElement >= 100)
        {
            FireResistance += 6;
            WaterResistance += 6;
            LightResistance += 6;
            DarkResistance += 6;
            MagicDefence += 5;
            Mp += 200;
            ElementRate += 2;
        }

        if (slElement >= 110)
        {
            ElementRate += 2;
            MagicDefence += 5;
        }

        if (slElement >= 115)
        {
            Mp += 200;
        }

        if (slElement < 120)
        {
            return;
        }

        ElementRate += 2;
        FireResistance += 5;
        WaterResistance += 5;
        LightResistance += 5;
        DarkResistance += 5;

        #endregion
    }
}