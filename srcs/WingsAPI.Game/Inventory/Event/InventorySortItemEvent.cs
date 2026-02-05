using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Inventory.Event;

public class InventorySortItemEvent : PlayerEvent
{
    public InventoryType InventoryType { get; init; }
    public bool Confirm { get; init; }
}