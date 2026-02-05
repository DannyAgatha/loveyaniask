using ProtoBuf;
using WingsEmu.DTOs.Items;

namespace WingsEmu.DTOs.Inventory;

[ProtoContract]
public class PartnerWarehouseItemDto
{
    [ProtoMember(1)]
    public short Slot { get; set; }

    [ProtoMember(2)]
    public ItemInstanceDTO ItemInstance { get; set; }
}