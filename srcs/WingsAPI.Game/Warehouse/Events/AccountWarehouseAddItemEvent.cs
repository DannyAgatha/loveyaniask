using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;

namespace WingsEmu.Game.Warehouse.Events;

public class AccountWarehouseAddItemEvent : PlayerEvent
{
    public InventoryItem Item { get; init; }
    public int Amount { get; init; }
    public short SlotDestination { get; init; }
}