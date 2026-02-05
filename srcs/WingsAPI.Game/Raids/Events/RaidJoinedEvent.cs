using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Raids.Enum;

namespace WingsEmu.Game.Raids.Events;

public class RaidJoinedEvent : PlayerEvent
{
    public RaidJoinType JoinType { get; init; }
}