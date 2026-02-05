using ProtoBuf;

namespace WingsAPI.Communication.Services.Requests
{
    [ProtoContract]
    public class ServiceExecuteGeneralEmergencyMaintenanceRequest
    {
        [ProtoMember(1)]
        public string Reason { get; init; }
    }
}