using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;

namespace WingsEmu.Game.Core.ItemHandling.Event;

public class InventoryUsedItemEvent : PlayerEvent
{
    public InventoryItem Item { get; init; }
}