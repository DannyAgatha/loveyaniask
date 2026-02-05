using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsEmu.Plugins.DistributedGameEvents.InterChannel
{
    [MessageType("bazaar.notification")]
    public class BazaarNotificationMessage : IMessage
    {
        public string OwnerName { get; set; }

        public int ItemVnum { get; set; }

        public int Amount { get; set; }
    }
}