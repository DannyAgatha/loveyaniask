using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Inventory.Event;

public class InventoryItemUsedEvent : PlayerEvent
{
    public int ItemVnum { get; init; }
}