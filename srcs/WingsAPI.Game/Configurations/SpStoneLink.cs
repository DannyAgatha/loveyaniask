using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WingsEmu.Game.Configurations;

[DataContract]
public class SpStoneLink
{
    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int StoneVnum { get; set; }

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public List<int> SpVnums { get; set; } = new();
}