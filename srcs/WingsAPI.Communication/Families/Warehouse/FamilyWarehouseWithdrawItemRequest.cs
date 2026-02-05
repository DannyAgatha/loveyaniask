using ProtoBuf;
using WingsAPI.Data.Families;

namespace WingsAPI.Communication.Families.Warehouse
{
    [ProtoContract]
    public class FamilyWarehouseWithdrawItemRequest
    {
        [ProtoMember(1)]
        public FamilyWarehouseItemDto ItemToWithdraw { get; set; }

        [ProtoMember(2)]
        public long? CharacterId { get; set; }

        [ProtoMember(3)]
        public int Amount { get; set; }

        [ProtoMember(4)]
        public string CharacterName { get; set; }
    }
}