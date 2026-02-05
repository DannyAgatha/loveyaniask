using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsAPI.Communication.Services.Messages
{
    [MessageType("service.order.kickall")]
    public class ServiceKickAllMessage : IMessage
    {
        public bool IsGlobal { get; init; }

        public string TargetedService { get; init; }
    }
}