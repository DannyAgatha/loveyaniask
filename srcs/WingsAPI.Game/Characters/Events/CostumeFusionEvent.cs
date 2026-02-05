using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;

namespace WingsEmu.Game.Characters.Events;

public class CostumeFusionEvent : PlayerEvent
{
    public InventoryItem LeftItem { get; init; }
    public InventoryItem RightItem { get; init; }
}