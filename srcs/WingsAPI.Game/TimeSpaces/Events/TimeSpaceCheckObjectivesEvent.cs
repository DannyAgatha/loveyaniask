using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.TimeSpaces.Events;

public class TimeSpaceCheckObjectivesEvent : PlayerEvent
{
    public TimeSpaceParty TimeSpaceParty { get; init; }
    public bool PlayerEnteredToEndPortal { get; init; }
    public bool SendMessageWithNotFinishedObjects { get; init; }
}