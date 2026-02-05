using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Inventory.Event;

public class InventoryTakeOffItemEvent : PlayerEvent
{
    public InventoryTakeOffItemEvent(short slot) => Slot = slot;

    public short Slot { get; }

    public bool ForceToRandomSlot { get; init; }
}