using System;
using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsAPI.Communication.Services.Messages
{
    [MessageType("service.status.down")]
    public class ServiceDownMessage : IMessage
    {
        public string ServiceName { get; init; }
        public DateTime LastUpdate { get; init; }
    }
}