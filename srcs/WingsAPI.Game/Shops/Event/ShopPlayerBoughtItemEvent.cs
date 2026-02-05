using WingsEmu.DTOs.Items;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Shops.Event;

public class ShopPlayerBoughtItemEvent : PlayerEvent
{
    public long SellerId { get; init; }
    public string SellerName { get; init; }
    public long TotalPrice { get; init; }
    public int Quantity { get; init; }
    public ItemInstanceDTO ItemInstance { get; init; }
}