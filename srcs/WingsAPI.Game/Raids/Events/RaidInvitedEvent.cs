using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Raids.Events;

public class RaidInvitedEvent : PlayerEvent
{
    public long TargetId { get; init; }
}