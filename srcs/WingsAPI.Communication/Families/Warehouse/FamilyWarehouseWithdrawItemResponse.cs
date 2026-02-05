using ProtoBuf;
using WingsAPI.Data.Families;
using WingsEmu.DTOs.Items;

namespace WingsAPI.Communication.Families.Warehouse
{
    [ProtoContract]
    public class FamilyWarehouseWithdrawItemResponse
    {
        [ProtoMember(1)]
        public RpcResponseType ResponseType { get; set; }

        [ProtoMember(2)]
        public FamilyWarehouseItemDto UpdatedItem { get; set; }

        [ProtoMember(3)]
        public ItemInstanceDTO WithdrawnItem { get; set; }
    }
}