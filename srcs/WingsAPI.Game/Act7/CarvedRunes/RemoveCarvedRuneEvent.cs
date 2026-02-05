using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;

namespace WingsEmu.Game.Act7.CarvedRunes;

public class RemoveCarvedRuneEvent : PlayerEvent
{
    public RemoveCarvedRuneEvent(InventoryItem inventoryItem) => Equipment = inventoryItem;
    public InventoryItem Equipment { get; set; }
}
