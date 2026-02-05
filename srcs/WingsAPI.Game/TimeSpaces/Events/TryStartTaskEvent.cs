using PhoenixLib.Events;

namespace WingsEmu.Game.TimeSpaces.Events;

public class TryStartTaskEvent : IAsyncEvent
{
    public TimeSpaceSubInstance Map { get; set; }
}