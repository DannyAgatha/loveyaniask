
using System;
using System.Collections.Generic;
using PhoenixLib.DAL;
using ProtoBuf;
using WingsAPI.Data.CarvedRune;
using WingsAPI.Packets.Enums;
using WingsEmu.DTOs.Skills;
using WingsEmu.Packets.Enums;

namespace WingsEmu.DTOs.Items;

[ProtoContract]
public class ItemInstanceDTO : IDto
{
    [ProtoMember(1)]
    public int Amount { get; set; }

    [ProtoMember(2)]
    public long? BoundCharacterId { get; set; }

    [ProtoMember(3)]
    public short Design { get; set; }

    [ProtoMember(4)]
    public int DurabilityPoint { get; set; }

    [ProtoMember(5)]
    public DateTime? ItemDeleteTime { get; set; }

    [ProtoMember(6)]
    public int ItemVNum { get; set; }

    [ProtoMember(7)]
    public short Rarity { get; set; }

    [ProtoMember(8)]
    public byte Upgrade { get; set; }

    [ProtoMember(9)]
    public byte Ammo { get; set; }

    [ProtoMember(10)]
    public byte Cellon { get; set; }

    [ProtoMember(16)]
    public short DarkResistance { get; set; }

    [ProtoMember(17)]
    public short ElementRate { get; set; }

    [ProtoMember(18)]
    public short FireResistance { get; set; }

    [ProtoMember(19)]
    public bool IsEmpty { get; set; }

    [ProtoMember(20)]
    public bool IsFixed { get; set; }

    [ProtoMember(21)]
    public short LightResistance { get; set; }

    [ProtoMember(22)]
    public short? ShellRarity { get; set; }

    [ProtoMember(23)]
    public short WaterResistance { get; set; }

    [ProtoMember(24)]
    public long Xp { get; set; }

    [ProtoMember(25)]
    public byte Agility { get; set; }

    [ProtoMember(26)]
    public bool PartnerSkill1 { get; set; }

    [ProtoMember(27)]
    public bool PartnerSkill2 { get; set; }

    [ProtoMember(28)]
    public bool PartnerSkill3 { get; set; }

    [ProtoMember(29)]
    public byte SkillRank1 { get; set; }

    [ProtoMember(30)]
    public byte SkillRank2 { get; set; }

    [ProtoMember(31)]
    public byte SkillRank3 { get; set; }

    [ProtoMember(32)]
    public List<EquipmentOptionDTO> EquipmentOptions { get; set; }

    [ProtoMember(33)]
    public short SlDamage { get; set; }

    [ProtoMember(34)]
    public short SlDefence { get; set; }

    [ProtoMember(35)]
    public short SlElement { get; set; }

    [ProtoMember(36)]
    public short SlHp { get; set; }

    [ProtoMember(37)]
    public byte SpDamage { get; set; }

    [ProtoMember(38)]
    public byte SpDark { get; set; }

    [ProtoMember(39)]
    public byte SpDefence { get; set; }

    [ProtoMember(40)]
    public byte SpElement { get; set; }

    [ProtoMember(41)]
    public byte SpFire { get; set; }

    [ProtoMember(42)]
    public byte SpHP { get; set; }

    [ProtoMember(43)]
    public byte SpLevel { get; set; }

    [ProtoMember(44)]
    public byte SpLight { get; set; }

    [ProtoMember(45)]
    public byte SpStoneUpgrade { get; set; }

    [ProtoMember(46)]
    public byte SpWater { get; set; }

    [ProtoMember(47)]
    public List<PartnerSkillDTO> PartnerSkills { get; set; }

    [ProtoMember(48)]
    public int? HoldingVNum { get; set; }

    [ProtoMember(49)]
    public MateType? MateType { get; set; }

    [ProtoMember(50)]
    public bool IsLimitedMatePearl { get; set; }

    [ProtoMember(51)]
    public short EquippedWingsVNum { get; set; }

    [ProtoMember(52)]
    public short AppearanceWingsVNum { get; set; }

    [ProtoMember(53)]
    public bool CanResetWingsAppearance { get; set; }

    [ProtoMember(54)]
    public byte SlotStatus { get; set; }

    [ProtoMember(55)]
    public byte Slot2Status { get; set; }

    [ProtoMember(56)]
    public byte Slot3Status { get; set; }

    [ProtoMember(57)]
    public byte SpAttackPassive { get; set; }

    [ProtoMember(58)]
    public byte SpCritProbabilityPassive { get; set; }

    [ProtoMember(59)]
    public byte SpPercentBonusPassive { get; set; }

    [ProtoMember(60)]
    public byte SpMeleeDefenceBonusPassive { get; set; }

    [ProtoMember(61)]
    public byte SpRangedDefenceBonusPassive { get; set; }

    [ProtoMember(62)]
    public byte SpMagicalDefenceBonusPassive { get; set; }

    [ProtoMember(63)]
    public byte WingsDesign { get; set; }
    
    [ProtoMember(64)]
    public int Skin { get; set; }
    
    [ProtoMember(65)]
    public byte SpGemElement { get; set; }

    [ProtoMember(80)]
    public ItemInstanceType Type { get; set; }

    [ProtoMember(81)]
    public int WeaponMinDamageAdditionalValue { get; set; }

    [ProtoMember(82)]
    public int WeaponMaxDamageAdditionalValue { get; set; }

    [ProtoMember(83)]
    public int WeaponHitRateAdditionalValue { get; set; }

    [ProtoMember(84)]
    public int ArmorDodgeAdditionalValue { get; set; }

    [ProtoMember(85)]
    public int ArmorRangeAdditionalValue { get; set; }

    [ProtoMember(86)]
    public int ArmorMagicAdditionalValue { get; set; }

    [ProtoMember(87)]
    public int ArmorMeleeAdditionalValue { get; set; }

    [ProtoMember(90)]
    public Guid? SerialTracker { get; set; }

    [ProtoMember(100)]
    public int? OriginalItemVnum { get; set; }

    [ProtoMember(101)]
    public bool IsSecondSpecialistSlotActivated { get; set; }

    [ProtoMember(102)]
    public bool IsThirdSpecialistSlotActivated { get; set; }

    [ProtoMember(103)]
    public byte CurrentActiveSpecialistSlot { get; set; }
    
    [ProtoMember(104)]
    public byte SpCriticalDefense { get; set; }
    
    [ProtoMember(105)]
    public Dictionary<int, int> PityCounter { get; set; } = new();
    
    [ProtoMember(106)]
    public byte Stars { get; set; }
    
    [ProtoMember(107)]
    public CarvedRunesDto CarvedRunes { get; set; }
    
    [ProtoMember(108)]
    public bool HasBeenFused { get; set; }
}