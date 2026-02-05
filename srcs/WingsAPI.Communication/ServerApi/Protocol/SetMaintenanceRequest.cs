using ProtoBuf;

namespace WingsAPI.Communication.ServerApi.Protocol
{
    [ProtoContract]
    public class SetMaintenanceRequest
    {
        [ProtoMember(1)]
        public bool Maintenance { get; init; }
    }
}