using ProtoBuf;
using WingsAPI.Data.Account;

namespace WingsAPI.Communication.DbServer.WarehouseService
{
    [ProtoContract]
    public class AccountWarehouseWithdrawItemRequest
    {
        [ProtoMember(1)]
        public AccountWarehouseItemDto ItemToWithdraw { get; set; }

        [ProtoMember(2)]
        public int Amount { get; set; }
    }
}