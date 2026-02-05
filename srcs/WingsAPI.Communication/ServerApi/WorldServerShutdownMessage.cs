using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsAPI.Communication.ServerApi
{
    [MessageType("worldserver.shutdown")]
    public class WorldServerShutdownMessage : IMessage
    {
        public int ChannelId { get; set; }
    }
}