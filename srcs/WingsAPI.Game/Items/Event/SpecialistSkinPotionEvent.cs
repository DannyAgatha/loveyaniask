using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;

namespace WingsEmu.Game.Items.Event;

public class SpecialistSkinPotionEvent : PlayerEvent
{
    public SpecialistSkinPotionEvent(InventoryItem item) => Item = item;

    public InventoryItem Item { get; }
}