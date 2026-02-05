using PhoenixLib.Events;

namespace WingsEmu.Game.TimeSpaces.Events;

public class TimeSpaceTaskCheckEvent : IAsyncEvent
{
    public bool Completed { get; set; }
}