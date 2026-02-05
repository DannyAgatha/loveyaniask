using ProtoBuf;
using WingsAPI.Data.Account;
using WingsEmu.DTOs.Items;

namespace WingsAPI.Communication.DbServer.WarehouseService
{
    [ProtoContract]
    public class AccountWarehouseWithdrawItemResponse
    {
        [ProtoMember(1)]
        public RpcResponseType ResponseType { get; set; }

        [ProtoMember(2)]
        public AccountWarehouseItemDto UpdatedItem { get; set; }

        [ProtoMember(3)]
        public ItemInstanceDTO WithdrawnItem { get; set; }
    }
}