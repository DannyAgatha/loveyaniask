// NosEmu
// 


using System.Runtime.Serialization;

namespace WingsEmu.Game.Configurations;

[DataContract]
public class GameRateConfiguration
{
    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int MobXpRate { get; set; } = 1;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int JobXpRate { get; set; } = 1;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int HeroXpRate { get; set; } = 1;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int FairyXpRate { get; set; } = 1;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int MateXpRate { get; set; } = 1;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int PartnerXpRate { get; set; } = 1;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int FamilyXpRate { get; set; } = 1;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int ReputRate { get; set; } = 1;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int MobDropRate { get; set; } = 1;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int MobDropChance { get; set; } = 1;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int GoldDropRate { get; set; } = 1;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int GoldRate { get; set; } = 1;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int GoldDropChance { get; set; } = 1;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int GenericDropRate { get; set; } = 1;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int GenericDropChance { get; set; } = 1;
}