using ProtoBuf;
using WingsAPI.Data.Account;

namespace WingsAPI.Communication.DbServer.WarehouseService
{
    [ProtoContract]
    public class AccountWarehouseMoveItemRequest
    {
        [ProtoMember(1)]
        public AccountWarehouseItemDto WarehouseItemDtoToMove { get; set; }

        [ProtoMember(2)]
        public int Amount { get; set; }

        [ProtoMember(3)]
        public short NewSlot { get; set; }
    }
}