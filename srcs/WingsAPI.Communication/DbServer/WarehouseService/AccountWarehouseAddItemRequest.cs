using ProtoBuf;
using WingsAPI.Data.Account;

namespace WingsAPI.Communication.DbServer.WarehouseService
{
    [ProtoContract]
    public class AccountWarehouseAddItemRequest
    {
        [ProtoMember(1)]
        public AccountWarehouseItemDto Item { get; set; }
    }
}