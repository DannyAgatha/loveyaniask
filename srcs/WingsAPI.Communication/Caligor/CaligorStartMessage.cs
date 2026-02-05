using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsAPI.Communication.Caligor
{
    [MessageType("caligor.start")]
    public class CaligorStartMessage : IMessage
    {
    }
}