using System;
using System.Collections.Generic;
using WingsAPI.Data.Families;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game._enum;
using WingsEmu.Game.BCard;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;

namespace WingsEmu.Game.Battle;

public interface IBattleEntityDump
{
    VisualType Type { get; }
    long Id { get; }
    AttackType AttackType { get; }
    ElementType Element { get; }
    int ElementRate { get; }

    int Morale { get; }

    IReadOnlyDictionary<(BCardType type, byte subType), List<BCardDTO>> BCards { get; }
    IReadOnlyDictionary<(BCardType type, byte subType), List<(int casterLevel, BCardDTO bCard)>> BuffBCards { get; }
    ISet<int> BuffsById { get; }
    IReadOnlyDictionary<FamilyUpgradeType, (short, short?)> FamilyUpgrades { get; }
    int BadBuffCounter { get; }
    int Level { get; }
    FactionType Faction { get; }

    int DamageMinimum { get; }
    int DamageMaximum { get; }

    int AttackUpgrade { get; }
    int DefenseUpgrade { get; }

    int HitRate { get; }

    int Hp { get; }
    int Mp { get; }
    int MaxHp { get; }
    int MaxMp { get; }

    int WeaponDamageMinimum { get; }
    int WeaponDamageMaximum { get; }
    int CriticalChance { get; }
    int CriticalDamage { get; }
    int CriticalDefence { get; }
    int FireResistance { get; }
    int WaterResistance { get; }
    int LightResistance { get; }
    int ShadowResistance { get; }
    int MeleeDefense { get; }
    int MeleeDodge { get; }
    int RangeDefense { get; }
    int RangeDodge { get; }

    int MagicalDefense { get; }
    Position Position { get; }
    IMapInstance MapInstance { get; }

    MonsterRaceType MonsterRaceType { get; }
    Enum MonsterRaceSubType { get; }
    IBCardDataComponent BCardDataComponent { get; }
}