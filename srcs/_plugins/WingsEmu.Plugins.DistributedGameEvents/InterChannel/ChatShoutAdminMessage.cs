using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsEmu.Plugins.DistributedGameEvents.InterChannel
{
    [MessageType("chat.shout.adminmessage")]
    public class ChatShoutAdminMessage : IMessage
    {
        public string Message { get; init; }
    }
}