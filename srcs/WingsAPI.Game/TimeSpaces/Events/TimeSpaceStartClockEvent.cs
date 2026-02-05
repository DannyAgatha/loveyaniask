using PhoenixLib.Events;

namespace WingsEmu.Game.TimeSpaces.Events;

public class TimeSpaceStartClockEvent : IAsyncEvent
{
    public TimeSpaceStartClockEvent(TimeSpaceParty timeSpaceParty, bool isVisible)
    {
        TimeSpaceParty = timeSpaceParty;
        IsVisible = isVisible;
    }

    public TimeSpaceParty TimeSpaceParty { get; set; }
    public bool IsVisible { get; set; }
}