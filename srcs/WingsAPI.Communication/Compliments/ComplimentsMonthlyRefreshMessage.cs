using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsAPI.Communication.Compliments
{
    [MessageType("compliments.refresh.monthly")]
    public class ComplimentsMonthlyRefreshMessage : IMessage
    {
        public bool Force { get; set; }
    }
}