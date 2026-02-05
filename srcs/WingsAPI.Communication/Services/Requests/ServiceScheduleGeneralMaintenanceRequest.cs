using System;
using ProtoBuf;

namespace WingsAPI.Communication.Services.Requests
{
    [ProtoContract]
    public class ServiceScheduleGeneralMaintenanceRequest
    {
        [ProtoMember(1)]
        public TimeSpan ShutdownTimeSpan { get; init; }

        [ProtoMember(2)]
        public string Reason { get; init; }
    }
}