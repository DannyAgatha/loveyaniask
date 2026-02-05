using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Bazaar.Events;

public class BazaarItemRemoveEvent : PlayerEvent
{
    public BazaarItemRemoveEvent(long bazaarItemId) => BazaarItemId = bazaarItemId;

    public long BazaarItemId { get; }
}