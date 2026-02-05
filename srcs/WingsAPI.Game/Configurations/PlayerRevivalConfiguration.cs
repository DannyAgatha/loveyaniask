using System;
using System.Runtime.Serialization;

namespace WingsEmu.Game.Configurations;

[DataContract]
public class PlayerRevivalConfiguration
{
    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public PlayerRevivalPenalization PlayerRevivalPenalization { get; set; } = new();

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public TimeSpan RevivalDialogDelay { get; set; } = TimeSpan.FromSeconds(2);

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public TimeSpan ForcedRevivalDelay { get; set; } = TimeSpan.FromSeconds(30);

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public TimeSpan Act4SealRevivalDelay { get; set; } = TimeSpan.FromSeconds(2);

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public TimeSpan Act4RevivalDelay { get; set; } = TimeSpan.FromSeconds(30);
    
    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public TimeSpan Act4CaligorRevivalDelay { get; set; } = TimeSpan.FromSeconds(20);
}