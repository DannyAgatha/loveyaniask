using WingsEmu.DTOs.Items;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Bazaar.Events;

public class BazaarItemBoughtEvent : PlayerEvent
{
    public long BazaarItemId { get; init; }
    public long SellerId { get; init; }
    public string SellerName { get; init; }
    public long PricePerItem { get; init; }
    public int Amount { get; init; }
    public ItemInstanceDTO BoughtItem { get; init; }
}