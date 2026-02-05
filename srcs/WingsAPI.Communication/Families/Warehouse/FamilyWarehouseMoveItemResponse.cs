using ProtoBuf;
using WingsAPI.Data.Families;

namespace WingsAPI.Communication.Families.Warehouse
{
    [ProtoContract]
    public class FamilyWarehouseMoveItemResponse
    {
        [ProtoMember(1)]
        public RpcResponseType ResponseType { get; set; }

        [ProtoMember(2)]
        public FamilyWarehouseItemDto OldItem { get; set; }

        [ProtoMember(3)]
        public FamilyWarehouseItemDto NewItem { get; set; }
    }
}