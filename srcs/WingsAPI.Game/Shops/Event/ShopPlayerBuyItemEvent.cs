using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Shops.Event;

public class ShopPlayerBuyItemEvent : PlayerEvent
{
    public long OwnerId { get; init; }
    public short Slot { get; init; }
    public int Amount { get; init; }
}