using WingsEmu.DTOs.Items;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Families.Event;

public class FamilyWarehouseItemPlacedEvent : PlayerEvent
{
    public ItemInstanceDTO ItemInstance { get; init; }
    public int Amount { get; init; }
    public short DestinationSlot { get; init; }
}