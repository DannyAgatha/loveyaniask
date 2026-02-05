using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Warehouse.Events;

public class PartnerWarehouseWithdrawEvent : PlayerEvent
{
    public PartnerWarehouseWithdrawEvent(short slot, int amount)
    {
        Slot = slot;
        Amount = amount;
    }

    public short Slot { get; }
    public int Amount { get; }
}