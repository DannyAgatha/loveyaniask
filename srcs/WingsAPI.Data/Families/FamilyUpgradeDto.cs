using System.Collections.Generic;
using ProtoBuf;

namespace WingsAPI.Data.Families;

[ProtoContract]
public class FamilyUpgradeDto
{
    [ProtoMember(1)]
    public HashSet<int> UpgradesBought { get; set; } = new();

    [ProtoMember(2)]
    public Dictionary<FamilyUpgradeType, (short, short?)> UpgradeValues { get; set; }
}