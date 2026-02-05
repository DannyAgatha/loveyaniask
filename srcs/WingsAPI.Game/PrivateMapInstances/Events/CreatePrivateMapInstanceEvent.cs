using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;

namespace WingsEmu.Game.PrivateMapInstances.Events;

public enum PrivateMapInstanceType
{
    SOLO = 0,
    GROUP = 1,
    FAMILY = 2,
    PUBLIC = 3
}

public class CreatePrivateMapInstanceEvent : PlayerEvent
{
    public InventoryItem Item { get; init; }
    public PrivateMapInstanceType Type { get; init; }
    public bool Confirm { get; init; }
    public bool IsPremium { get; set; }
    
    public bool IsHardcore { get; set; }
}