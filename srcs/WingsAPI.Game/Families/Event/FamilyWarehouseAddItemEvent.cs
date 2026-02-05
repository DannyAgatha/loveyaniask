using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;

namespace WingsEmu.Game.Families.Event;

public class FamilyWarehouseAddItemEvent : PlayerEvent
{
    public InventoryItem Item { get; init; }
    public int Amount { get; init; }
    public short DestinationSlot { get; init; }
}