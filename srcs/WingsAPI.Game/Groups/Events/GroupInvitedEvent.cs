using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Groups.Events;

public class GroupInvitedEvent : PlayerEvent
{
    public long TargetId { get; init; }
}