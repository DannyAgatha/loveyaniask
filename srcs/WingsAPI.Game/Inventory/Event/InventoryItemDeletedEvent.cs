using WingsEmu.DTOs.Items;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Inventory.Event;

public class InventoryItemDeletedEvent : PlayerEvent
{
    public ItemInstanceDTO ItemInstance { get; init; }
    public int ItemAmount { get; init; }
}