using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Warehouse.Events;

public class AccountWarehouseMoveEvent : PlayerEvent
{
    public AccountWarehouseMoveEvent(short originalSlot, int amount, short newSlot)
    {
        OriginalSlot = originalSlot;
        Amount = amount;
        NewSlot = newSlot;
    }

    public short OriginalSlot { get; }
    public int Amount { get; }
    public short NewSlot { get; }
}