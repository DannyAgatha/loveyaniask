using System.Collections.Generic;
using WingsEmu.DTOs.Items;
using WingsEmu.Packets.Enums.Character;

namespace WingsEmu.Game.Configurations.InitialConfiguration;

public class ClassEquipmentConfiguration
{
    public ClassType ClassType { get; set; }
    public List<EquipmentConfiguration> Equipments { get; set; }
    public List<SpecialistConfiguration> Specialists { get; set; }
    public List<SkillConfiguration> DefaultSkills { get; set; }

    public List<SkillConfiguration> PassiveSkills { get; set; }
}

public class EquipmentConfiguration
{
    public int ItemVnum { get; set; }
    public int Amount { get; set; }
    public byte Upgrade { get; set; }
    public sbyte Rare { get; set; }
    public bool Equipped { get; set; }
    public List<EquipmentOptionDTO> Options { get; set; }
}

public class SpecialistConfiguration
{
    public int CardVnum { get; set; }
    public int SpLevel { get; set; }
    public byte Upgrade { get; set; }
    public byte SpStoneUpgrade { get; set; }
    public byte SpDamage { get; set; }
    public byte SpDefence { get; set; }
    public byte SpElement { get; set; }
    public byte SpHp { get; set; }
    public byte SpFire { get; set; }
    public byte SpWater { get; set; }
    public byte SpLight { get; set; }
    public byte SpDark { get; set; }
    public bool Equipped { get; set; }
}

public class SkillConfiguration
{
    public int SkillVnum { get; set; }
}
public class InitialClassConfiguration
{
    public List<ClassEquipmentConfiguration> ClassEquipments { get; set; }
}