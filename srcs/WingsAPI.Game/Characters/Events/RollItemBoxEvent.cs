using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;

namespace WingsEmu.Game.Characters.Events;

public class RollItemBoxEvent : PlayerEvent
{
    public InventoryItem Item { get; set; }
}