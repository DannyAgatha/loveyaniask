using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;

namespace WingsEmu.Game.Characters.Events;

public class BankOpenEvent : PlayerEvent
{
    public long? NpcId { get; init; }
    public InventoryItem BankCard { get; init; }
}