using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Relations;

public class RelationBlockEvent : PlayerEvent
{
    public long CharacterId { get; init; }
}