using ProtoBuf;
using WingsEmu.DTOs.Items;

namespace WingsAPI.Data.Families;

[ProtoContract]
public class FamilyWarehouseItemDto
{
    [ProtoMember(1)]
    public long FamilyId { get; set; }

    [ProtoMember(2)]
    public short Slot { get; set; }

    [ProtoMember(3)]
    public ItemInstanceDTO ItemInstance { get; set; }
}