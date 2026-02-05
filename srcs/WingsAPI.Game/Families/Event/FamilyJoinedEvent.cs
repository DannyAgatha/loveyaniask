using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Families.Event;

public class FamilyJoinedEvent : PlayerEvent
{
    public long FamilyId { get; init; }
    public long InviterId { get; init; }
}