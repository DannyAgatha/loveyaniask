using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Exchange.Event;

public class TradeRequestedEvent : PlayerEvent
{
    public long TargetId { get; init; }
}