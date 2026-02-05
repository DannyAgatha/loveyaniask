using WingsEmu.DTOs.Items;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Shops.Event;

public class ShopNpcBoughtItemEvent : PlayerEvent
{
    public long SellerId { get; init; }
    public CurrencyType CurrencyType { get; init; }
    public long TotalPrice { get; init; }
    public ItemInstanceDTO ItemInstance { get; init; }
    public int Quantity { get; init; }
}