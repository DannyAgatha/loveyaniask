using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;

namespace WingsEmu.Game.Act4.Event;

public class Act4PutFlagEvent : PlayerEvent
{
    public InventoryItem InventoryItem { get; init; }
}