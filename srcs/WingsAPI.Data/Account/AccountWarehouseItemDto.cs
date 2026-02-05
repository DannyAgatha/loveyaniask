using ProtoBuf;
using WingsEmu.DTOs.Items;

namespace WingsAPI.Data.Account;

[ProtoContract]
public class AccountWarehouseItemDto
{
    [ProtoMember(1)]
    public long AccountId { get; set; }

    [ProtoMember(2)]
    public short Slot { get; set; }

    [ProtoMember(3)]
    public ItemInstanceDTO ItemInstance { get; set; }
}