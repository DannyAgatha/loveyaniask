using WingsEmu.DTOs.Items;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Shops.Event;

public class ShopNpcSoldItemEvent : PlayerEvent
{
    public ItemInstanceDTO ItemInstance { get; init; }
    public int Amount { get; init; }
    public long PricePerItem { get; init; }
}