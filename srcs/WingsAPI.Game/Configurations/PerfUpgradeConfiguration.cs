using System.Runtime.Serialization;
using WingsEmu.Core;

namespace WingsEmu.Game.Configurations;

[DataContract]
public class PerfUpgradeConfiguration
{
    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public Range<int> SpPerfUpgradeRange { get; set; }

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int GoldNeeded { get; set; }

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int StonesNeeded { get; set; }

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public byte SuccessChance { get; set; }

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public Range<int> StatAmountRange { get; set; }
}