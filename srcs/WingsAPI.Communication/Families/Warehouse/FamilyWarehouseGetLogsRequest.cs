using ProtoBuf;

namespace WingsAPI.Communication.Families.Warehouse
{
    [ProtoContract]
    public class FamilyWarehouseGetLogsRequest
    {
        [ProtoMember(1)]
        public long FamilyId { get; set; }

        [ProtoMember(2)]
        public long? CharacterId { get; set; }
    }
}