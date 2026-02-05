// NosEmu
// 
// Developed by NosWings Team

using ProtoBuf;

namespace WingsEmu.DTOs.Items;

[ProtoContract]
public class ItemRestrictionDTO
{
    [ProtoMember(1)]
    public int ItemVnum { get; set; }

    [ProtoMember(2)]
    public int Amount { get; set; }
}