using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Families.Event;

public class FamilyWarehouseShowItemEvent : PlayerEvent
{
    public short Slot { get; init; }
}