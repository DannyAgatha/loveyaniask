using PhoenixLib.Events;

namespace WingsEmu.Game.Act4.Event;

public class Act4DungeonBroadcastBossOpenEvent : IAsyncEvent
{
    public DungeonInstance DungeonInstance { get; init; }
}