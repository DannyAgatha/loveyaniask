using System.Collections.Generic;
using ProtoBuf;
using WingsAPI.Data.Families;

namespace WingsAPI.Communication.Families.Warehouse
{
    [ProtoContract]
    public class FamilyWarehouseGetLogsResponse
    {
        [ProtoMember(1)]
        public RpcResponseType ResponseType { get; set; }

        [ProtoMember(2)]
        public IList<FamilyWarehouseLogEntryDto> Logs { get; set; }
    }
}