using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsEmu.Plugins.DistributedGameEvents.PlayerEvents
{
    [MessageType("kick.account")]
    public class KickAccountMessage : IMessage
    {
        public long AccountId { get; set; }
    }
}