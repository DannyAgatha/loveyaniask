using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Shops.Event;

public class ShopOpenedEvent : PlayerEvent
{
    public string ShopName { get; init; }
}