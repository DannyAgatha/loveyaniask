using System.Collections.Generic;
using WingsEmu.DTOs.Items;
using WingsEmu.DTOs.Skills;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Configurations.InitialConfiguration;

public class MateConfiguration
{
    public int NpcVnum { get; set; }
    public byte Level { get; set; }
    public byte HeroLevel { get; set; }
    public byte PetSlot { get; set; }
    public byte Stars { get; set; }
    
    public byte Attack { get; set; }
    
    public byte Defence { get; set; }
    public MateType MateType { get; set; }
    public bool IsTeamMember { get; set; }
    public List<ItemConfiguration> Items { get; set; }
}

public class ItemConfiguration
{
    public int ItemVnum { get; set; }
    public byte Upgrade { get; set; }
    public sbyte Rarity { get; set; }
    public bool Equipped { get; set; } 
    public ItemInstanceType ItemInstanceType { get; set; }
    public int OriginalItemVnum { get; set; }
    public int WeaponHitRateAdditionalValue { get; set; }
    public int WeaponMaxDamageAdditionalValue { get; set; }
    public int WeaponMinDamageAdditionalValue { get; set; }
    public int ArmorDodgeAdditionalValue { get; set; }
    public int ArmorRangeAdditionalValue { get; set; }
    public int ArmorMagicAdditionalValue { get; set; }
    public int ArmorMeleeAdditionalValue { get; set; }
    public short FireResistance { get; set; }
    public short LightResistance { get; set; }
    public short WaterResistance { get; set; }
    public short DarkResistance { get; set; }
    public byte Agility { get; set; }
    public bool PartnerSkill1 { get; set; }
    public bool PartnerSkill2 { get; set; }
    public bool PartnerSkill3 { get; set; }
    public List<PartnerSkillDTO> PartnerSkills { get; set; } = [];
    public byte SkillRank1 { get; set; }
    public byte SkillRank2 { get; set; }
    public byte SkillRank3 { get; set; }
    public byte SpDamage { get; set; }
    public byte SpDefence { get; set; }
    public byte SpCriticalDefense { get; set; }
    public byte SpHp { get; set; }
    public byte SpFire { get; set; }
    public byte SpLight { get; set; }
    public byte SpWater { get; set; }
    public byte SpDark { get; set; }
    public byte SpStoneUpgrade { get; set; }
}

public class InitialMateConfiguration
{
    public List<MateConfiguration> Mates { get; set; } = [];
}