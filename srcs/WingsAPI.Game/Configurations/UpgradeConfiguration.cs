using System.Collections.Generic;
using System.Runtime.Serialization;
using WingsEmu.Core;

namespace WingsEmu.Game.Configurations;

[DataContract]
public class UpgradeConfiguration
{
    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public Range<byte> SpUpgradeRange { get; set; }

    [DataMember(EmitDefaultValue = true, IsRequired = false)]
    public byte SpLevelNeeded { get; set; }

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public short DestroyChance { get; set; }

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public short SuccessChance { get; set; }

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public long GoldNeeded { get; set; }

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int FeatherNeeded { get; set; }

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int FullMoonsNeeded { get; set; }

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int ScrollVnum { get; set; }

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public List<SpecialItem> SpecialItemsNeeded { get; set; } = new()
    {
        new()
    };
}