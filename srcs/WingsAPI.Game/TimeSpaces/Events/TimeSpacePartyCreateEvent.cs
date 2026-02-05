using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Inventory;

namespace WingsEmu.Game.TimeSpaces.Events;

public class TimeSpacePartyCreateEvent : PlayerEvent
{
    public TimeSpacePartyCreateEvent(long timeSpaceId, InventoryItem itemToRemove = null, bool isEasyMode = false, bool isChallengeMode = false)
    {
        TimeSpaceId = timeSpaceId;
        ItemToRemove = itemToRemove;
        IsEasyMode = isEasyMode;
        IsChallengeMode = isChallengeMode;
    }

    public long TimeSpaceId { get; }
    public bool IsEasyMode { get; }
    public bool IsChallengeMode { get; }
    public InventoryItem ItemToRemove { get; }
}