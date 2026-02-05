using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Families.Event;

public class FamilyInvitedEvent : PlayerEvent
{
    public long TargetId { get; init; }
}