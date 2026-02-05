using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Shops.Event;

public class ShopNpcListItemsEvent : PlayerEvent
{
    public int NpcId { get; set; }
    public byte ShopType { get; set; }
}