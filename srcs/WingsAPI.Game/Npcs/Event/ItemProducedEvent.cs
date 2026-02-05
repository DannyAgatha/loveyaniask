using WingsEmu.DTOs.Items;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Npcs.Event;

public class ItemProducedEvent : PlayerEvent
{
    public ItemInstanceDTO ItemInstance { get; init; }
    public int ItemAmount { get; init; }
}