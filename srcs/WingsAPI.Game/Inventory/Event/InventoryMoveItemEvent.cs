using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Inventory.Event;

public class InventoryMoveItemEvent : PlayerEvent
{
    public InventoryMoveItemEvent(InventoryType inventoryType, short slot, int amount, short destinationSlot, InventoryType destinationType, bool sendPackets = true)
    {
        InventoryType = inventoryType;
        Slot = slot;
        Amount = amount;
        DestinationSlot = destinationSlot;
        DestinationType = destinationType;
        SendPackets = sendPackets;
    }

    public InventoryType InventoryType { get; }
    public short Slot { get; }
    public int Amount { get; }
    public short DestinationSlot { get; }
    public InventoryType DestinationType { get; }
    public bool SendPackets { get; }
}