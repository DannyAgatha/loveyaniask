using PhoenixLib.Events;

namespace WingsEmu.Game.Act4.Event;

public class Act4DungeonBroadcastPacketEvent : IAsyncEvent
{
    public DungeonInstance DungeonInstance { get; init; }
}