using WingsEmu.DTOs.Items;
using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Helpers;

namespace WingsEmu.Game.Inventory.Event;

public class InventoryPickedUpPlayerItemEvent : PlayerEvent
{
    public ItemInstanceDTO ItemInstance { get; init; }
    public int Amount { get; init; }
    public Location Location { get; init; }
}