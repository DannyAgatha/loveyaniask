using System.Runtime.Serialization;

namespace WingsEmu.Game.Configurations;

[DataContract]
public class GameMinMaxConfiguration
{
    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public short MaxLevel { get; set; } = 99;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public short MaxMateLevel { get; set; } = 99;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public short MaxJobLevel { get; set; } = 80;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public short MaxSpLevel { get; set; } = 99;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public short MaxHeroLevel { get; set; } = 60;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public short HeroMinLevel { get; set; } = 88;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public short MinLodLevel { get; set; } = 55;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int MaxGold { get; set; } = 1_000_000_000;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public long MaxBankGold { get; set; } = 100_000_000_000;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public byte MaxBotCodeAttempts { get; set; } = 3;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public float MaxDignity { get; set; } = 200;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public float MinDignity { get; set; } = -1000;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public long MaxReputation { get; set; } = long.MaxValue;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public long MinReputation { get; set; }

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public short MaxMateLoyalty { get; set; } = 1000;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public short MinMateLoyalty { get; set; }

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public short MaxNpcTalkRange { get; set; } = 4;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int MaxSpAdditionalPoints { get; set; } = 1_000_000;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public short MaxSpBasePoints { get; set; } = 1000;

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int MaxTrophyFragments { get; set; } = 100;
}