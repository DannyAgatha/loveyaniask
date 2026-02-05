using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Helpers;

namespace WingsEmu.Game.Inventory.Event;

public class InventoryPickedUpItemEvent : PlayerEvent
{
    public int ItemVnum { get; init; }
    public int Amount { get; init; }
    public Location Location { get; init; }
}