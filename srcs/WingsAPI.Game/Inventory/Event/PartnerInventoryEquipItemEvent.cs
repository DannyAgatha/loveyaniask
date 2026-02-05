using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Inventory.Event;

public class PartnerInventoryEquipItemEvent : PlayerEvent
{
    public PartnerInventoryEquipItemEvent(short partnerSlot, byte slot, InventoryType inventoryType = InventoryType.Equipment)
    {
        PartnerSlot = partnerSlot;
        Slot = slot;
        InventoryType = inventoryType;
    }

    public short PartnerSlot { get; }
    public byte Slot { get; }
    public InventoryType InventoryType { get; }
}