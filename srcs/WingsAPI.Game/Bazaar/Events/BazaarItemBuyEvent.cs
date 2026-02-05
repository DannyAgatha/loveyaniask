using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Bazaar.Events;

public class BazaarItemBuyEvent : PlayerEvent
{
    public BazaarItemBuyEvent(long bazaarItemId, int amount, long pricePerItem)
    {
        BazaarItemId = bazaarItemId;
        Amount = amount;
        PricePerItem = pricePerItem;
    }

    public long BazaarItemId { get; }

    public int Amount { get; }

    public long PricePerItem { get; }
}