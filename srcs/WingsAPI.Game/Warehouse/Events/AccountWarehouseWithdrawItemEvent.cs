using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Warehouse.Events;

public class AccountWarehouseWithdrawItemEvent : PlayerEvent
{
    public AccountWarehouseWithdrawItemEvent(short slot, int amount)
    {
        Slot = slot;
        Amount = amount;
    }

    public short Slot { get; }

    public int Amount { get; }
}