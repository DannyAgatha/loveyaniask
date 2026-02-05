using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Shops.Event;

public class BuyShopItemEvent : PlayerEvent
{
    public long OwnerId { get; set; }
    public short Slot { get; set; }
    public int Amount { get; set; }
}