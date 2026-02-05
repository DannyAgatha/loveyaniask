using WingsEmu.DTOs.Items;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Bazaar.Events;

public class BazaarItemExpiredEvent : PlayerEvent
{
    public long BazaarItemId { get; init; }
    public long Price { get; init; }
    public int Quantity { get; init; }
    public ItemInstanceDTO Item { get; init; }
}