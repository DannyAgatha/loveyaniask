using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Inventory.Event;

public class ItemSumEvent : PlayerEvent
{
    public ItemSumEvent(InventoryItem leftItem, InventoryItem rightItem)
    {
        LeftItem = leftItem;
        RightItem = rightItem;
    }

    public InventoryItem LeftItem { get; }
    public InventoryItem RightItem { get; }
}