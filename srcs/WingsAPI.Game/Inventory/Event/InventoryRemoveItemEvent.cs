using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Inventory.Event;

public class InventoryRemoveItemEvent : PlayerEvent
{
    public InventoryRemoveItemEvent(int itemVnum, int amount = 1, bool isEquipped = false, InventoryItem inventoryItem = null, bool sendPackets = true)
    {
        ItemVnum = itemVnum;
        Amount = amount;
        IsEquipped = isEquipped;
        InventoryItem = inventoryItem;
        SendPackets = sendPackets;
    }

    public int ItemVnum { get; }
    public int Amount { get; }
    public bool IsEquipped { get; }
    public InventoryItem InventoryItem { get; }
    public bool SendPackets { get; }
}