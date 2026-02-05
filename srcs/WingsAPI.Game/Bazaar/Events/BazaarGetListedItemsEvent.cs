using WingsAPI.Packets.Enums.Bazaar;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Bazaar.Events;

public class BazaarGetListedItemsEvent : PlayerEvent
{
    public BazaarGetListedItemsEvent(ushort index, BazaarListedItemType filter)
    {
        Index = index;
        Filter = filter;
    }

    public ushort Index { get; }
    public BazaarListedItemType Filter { get; }
}