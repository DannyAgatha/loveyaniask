using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.TimeSpaces.Events;

public class TimeSpaceStartTaskEvent : PlayerEvent
{
    public TimeSpaceSubInstance TimeSpaceSubInstance { get; set; }
}