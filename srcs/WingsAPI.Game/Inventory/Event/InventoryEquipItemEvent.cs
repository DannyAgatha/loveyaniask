using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Inventory.Event;

public class InventoryEquipItemEvent : PlayerEvent
{
    public InventoryEquipItemEvent(short slot, bool isSpecialType = false, InventoryType? inventoryType = null, bool boundItem = false)
    {
        Slot = slot;
        IsSpecialType = isSpecialType;
        InventoryType = inventoryType;
        BoundItem = boundItem;
    }

    public short Slot { get; }
    public bool IsSpecialType { get; }
    public InventoryType? InventoryType { get; }
    public bool BoundItem { get; }
}