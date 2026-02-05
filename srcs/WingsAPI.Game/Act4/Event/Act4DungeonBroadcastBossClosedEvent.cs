using PhoenixLib.Events;

namespace WingsEmu.Game.Act4.Event;

public class Act4DungeonBroadcastBossClosedEvent : IAsyncEvent
{
    public DungeonInstanceWrapper DungeonInstanceWrapper { get; init; }
}