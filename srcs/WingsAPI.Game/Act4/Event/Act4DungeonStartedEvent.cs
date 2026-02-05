using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Act4.Event;

public class Act4DungeonStartedEvent : PlayerEvent
{
    public FactionType FactionType { get; init; }
    public DungeonType DungeonType { get; init; }
}