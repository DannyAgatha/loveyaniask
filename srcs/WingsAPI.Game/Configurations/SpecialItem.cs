using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WingsEmu.Game.Configurations;

[DataContract]
public class SpecialItem
{
    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int ItemVnum { get; set; }

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public int Amount { get; set; }

    [DataMember(EmitDefaultValue = true, IsRequired = false)]
    public List<int> SpVnums { get; set; } = new();
}