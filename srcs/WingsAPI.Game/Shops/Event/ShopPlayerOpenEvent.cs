using System.Collections.Generic;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Shops.Event;

public class ShopPlayerOpenEvent : PlayerEvent
{
    public List<ShopPlayerItem> Items { get; init; }

    public string ShopTitle { get; init; }
}