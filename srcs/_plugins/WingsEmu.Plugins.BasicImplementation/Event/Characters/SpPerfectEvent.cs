using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class SpPerfectEvent : PlayerEvent
{
    public SpPerfectEvent(InventoryItem inventoryItem, bool isAutoUpgrade = false)
    {
        InventoryItem = inventoryItem;
        IsAutoUpgrade = isAutoUpgrade;
    }

    public InventoryItem InventoryItem { get; }
    public bool IsAutoUpgrade { get; }
}