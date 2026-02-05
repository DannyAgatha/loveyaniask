using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Warehouse.Events;

public class PartnerWarehouseDepositEvent : PlayerEvent
{
    public PartnerWarehouseDepositEvent(InventoryType inventoryType, short inventorySlot, int amount, short slotDestination)
    {
        InventoryType = inventoryType;
        InventorySlot = inventorySlot;
        Amount = amount;
        SlotDestination = slotDestination;
    }

    public InventoryType InventoryType { get; }
    public short InventorySlot { get; }
    public int Amount { get; }
    public short SlotDestination { get; }
}