using ProtoBuf;
using WingsAPI.Data.Families;

namespace WingsAPI.Communication.Families.Warehouse
{
    [ProtoContract]
    public class FamilyWarehouseAddItemRequest
    {
        [ProtoMember(1)]
        public FamilyWarehouseItemDto Item { get; set; }

        [ProtoMember(2)]
        public long? CharacterId { get; set; }

        [ProtoMember(3)]
        public string CharacterName { get; set; }
    }
}