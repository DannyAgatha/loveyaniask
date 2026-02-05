using ProtoBuf;

namespace WingsAPI.Communication.DbServer.WarehouseService
{
    [ProtoContract]
    public class AccountWarehouseGetItemRequest
    {
        [ProtoMember(1)]
        public long AccountId { get; set; }

        [ProtoMember(2)]
        public short Slot { get; set; }
    }
}