using ProtoBuf;

namespace WingsAPI.Communication.DbServer.WarehouseService
{
    [ProtoContract]
    public class AccountWarehouseGetItemsRequest
    {
        [ProtoMember(1)]
        public long AccountId { get; set; }
    }
}