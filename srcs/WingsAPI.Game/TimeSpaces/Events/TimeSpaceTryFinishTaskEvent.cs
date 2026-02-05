using PhoenixLib.Events;

namespace WingsEmu.Game.TimeSpaces.Events;

public class TimeSpaceTryFinishTaskEvent : IAsyncEvent
{
    public TimeSpaceTryFinishTaskEvent(TimeSpaceSubInstance timeSpaceSubInstance, TimeSpaceParty timeSpaceParty)
    {
        TimeSpaceSubInstance = timeSpaceSubInstance;
        TimeSpaceParty = timeSpaceParty;
    }

    public TimeSpaceSubInstance TimeSpaceSubInstance { get; }
    public TimeSpaceParty TimeSpaceParty { get; }
}