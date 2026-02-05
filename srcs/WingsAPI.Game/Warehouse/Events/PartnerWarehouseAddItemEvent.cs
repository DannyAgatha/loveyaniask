using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Items;

namespace WingsEmu.Game.Warehouse.Events;

public class PartnerWarehouseAddItemEvent : PlayerEvent
{
    public GameItemInstance ItemInstance { get; init; }
}