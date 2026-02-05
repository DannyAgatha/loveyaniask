using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Inventory.Event;

public class PartnerInventoryTakeOffItemEvent : PlayerEvent
{
    public PartnerInventoryTakeOffItemEvent(short petId, byte slot)
    {
        PetId = petId;
        Slot = slot;
    }

    public short PetId { get; }
    public byte Slot { get; }
}