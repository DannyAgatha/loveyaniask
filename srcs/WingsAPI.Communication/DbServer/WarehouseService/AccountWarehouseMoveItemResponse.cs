using ProtoBuf;
using WingsAPI.Data.Account;

namespace WingsAPI.Communication.DbServer.WarehouseService
{
    [ProtoContract]
    public class AccountWarehouseMoveItemResponse
    {
        [ProtoMember(1)]
        public RpcResponseType ResponseType { get; set; }

        [ProtoMember(2)]
        public AccountWarehouseItemDto OldItem { get; set; }

        [ProtoMember(3)]
        public AccountWarehouseItemDto NewItem { get; set; }
    }
}