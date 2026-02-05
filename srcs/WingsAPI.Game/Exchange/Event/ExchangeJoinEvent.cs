using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Exchange.Event;

public class ExchangeJoinEvent : PlayerEvent
{
    public IClientSession Target { get; set; }
}