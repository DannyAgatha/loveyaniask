using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.TimeSpaces.Events;

public class TimeSpaceLeavePartyEvent : PlayerEvent
{
    public bool RemoveLive { get; init; }
    public bool CheckForSeeds { get; init; }
    public bool CheckFinished { get; init; }
}