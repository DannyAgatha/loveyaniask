using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Inventory.Event;

public class PlayerItemToPartnerItemEvent : PlayerEvent
{
    public PlayerItemToPartnerItemEvent(short slot, InventoryType inventoryType)
    {
        Slot = slot;
        InventoryType = inventoryType;
    }

    public short Slot { get; }
    public InventoryType InventoryType { get; }
}