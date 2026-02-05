using ProtoBuf;
using WingsAPI.Data.Families;

namespace WingsAPI.Communication.Families.Warehouse
{
    [ProtoContract]
    public class FamilyWarehouseMoveItemRequest
    {
        [ProtoMember(1)]
        public FamilyWarehouseItemDto WarehouseItemDtoToMove { get; set; }

        [ProtoMember(2)]
        public long? CharacterId { get; set; }

        [ProtoMember(3)]
        public int Amount { get; set; }

        [ProtoMember(4)]
        public short NewSlot { get; set; }
    }
}