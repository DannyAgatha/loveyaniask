using PhoenixLib.Events;

namespace WingsEmu.Game.TimeSpaces.Events;

public class TimeSpaceDespawnMonstersInRoomEvent : IAsyncEvent
{
    public TimeSpaceSubInstance TimeSpaceSubInstance { get; init; }
}