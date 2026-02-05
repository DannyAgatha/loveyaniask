using PhoenixLib.Events;

namespace WingsEmu.Game.TimeSpaces.Events;

public class TimeSpaceDestroyEvent : IAsyncEvent
{
    public TimeSpaceDestroyEvent(TimeSpaceParty timeSpace) => TimeSpace = timeSpace;

    public TimeSpaceParty TimeSpace { get; }
}