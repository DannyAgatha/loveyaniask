// NosEmu
// 


using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;

namespace WingsEmu.Game.Raids.Events;

public class RaidPartyCreateEvent : PlayerEvent
{
    public RaidPartyCreateEvent(byte raidType, InventoryItem itemToRemove, bool isMarathonMode)
    {
        RaidType = raidType;
        ItemToRemove = itemToRemove;
        IsMarathonMode = isMarathonMode;
    }

    public byte RaidType { get; }
    public InventoryItem ItemToRemove { get; }
    public bool IsMarathonMode { get; } 
}