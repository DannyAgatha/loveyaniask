using ProtoBuf;
using WingsAPI.Data.Families;

namespace WingsAPI.Communication.Families.Warehouse
{
    [ProtoContract]
    public class FamilyWarehouseAddItemResponse
    {
        [ProtoMember(1)]
        public RpcResponseType ResponseType { get; set; }

        [ProtoMember(2)]
        public FamilyWarehouseItemDto Item { get; set; }
    }
}