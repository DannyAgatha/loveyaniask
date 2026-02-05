using System.Runtime.Serialization;

namespace WingsEmu.Game.Configurations;

[DataContract]
public class GameRevivalConfiguration
{
    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public PlayerRevivalConfiguration PlayerRevivalConfiguration { get; set; } = new();

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public MateRevivalConfiguration MateRevivalConfiguration { get; set; } = new();
}