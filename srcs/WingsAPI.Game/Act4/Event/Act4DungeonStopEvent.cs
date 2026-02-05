using PhoenixLib.Events;

namespace WingsEmu.Game.Act4.Event;

public class Act4DungeonStopEvent : IAsyncEvent
{
    public DungeonInstance DungeonInstance { get; init; }
}