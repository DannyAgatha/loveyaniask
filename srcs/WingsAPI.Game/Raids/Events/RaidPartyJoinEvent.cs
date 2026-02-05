using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Raids.Events;

public class RaidPartyJoinEvent : PlayerEvent
{
    public RaidPartyJoinEvent(long raidOwnerId, bool isByRaidList, bool isRaidMarathonMode = false)
    {
        RaidOwnerId = raidOwnerId;
        IsByRaidList = isByRaidList;
        IsRaidMarathonMode = isRaidMarathonMode;
    }

    public long RaidOwnerId { get; }
    public bool IsByRaidList { get; }
    public bool IsRaidMarathonMode { get; }
}