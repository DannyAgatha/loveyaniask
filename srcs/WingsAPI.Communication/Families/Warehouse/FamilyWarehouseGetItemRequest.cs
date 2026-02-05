using ProtoBuf;

namespace WingsAPI.Communication.Families.Warehouse
{
    [ProtoContract]
    public class FamilyWarehouseGetItemRequest
    {
        [ProtoMember(1)]
        public long FamilyId { get; set; }

        [ProtoMember(2)]
        public long? CharacterId { get; set; }

        [ProtoMember(3)]
        public short Slot { get; set; }
    }
}