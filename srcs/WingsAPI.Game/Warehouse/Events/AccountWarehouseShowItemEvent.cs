using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Warehouse.Events;

public class AccountWarehouseShowItemEvent : PlayerEvent
{
    public short Slot { get; init; }
}