using PhoenixLib.Events;

namespace WingsEmu.Game.Act4.Event;

public class Act4DungeonBossMapCleanUpEvent : IAsyncEvent
{
    public DungeonInstance DungeonInstance { get; init; }
    public DungeonSubInstance BossMap { get; init; }
}