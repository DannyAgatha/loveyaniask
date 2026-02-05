using WingsEmu.DTOs.Items;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Bazaar.Events;

public class BazaarItemInsertedEvent : PlayerEvent
{
    public long BazaarItemId { get; init; }
    public long Price { get; init; }
    public int Quantity { get; init; }
    public long Taxes { get; init; }
    public ItemInstanceDTO ItemInstance { get; init; }
}