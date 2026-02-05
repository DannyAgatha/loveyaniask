using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Inventory.Event;

public class InventoryPickUpItemEvent : PlayerEvent
{
    public InventoryPickUpItemEvent(VisualType pickerVisualType, long pickerId, long dropId)
    {
        PickerVisualType = pickerVisualType;
        PickerId = pickerId;
        DropId = dropId;
    }

    public VisualType PickerVisualType { get; }
    public long PickerId { get; }
    public long DropId { get; }
}