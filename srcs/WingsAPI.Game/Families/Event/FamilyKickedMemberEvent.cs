using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Families.Event;

public class FamilyKickedMemberEvent : PlayerEvent
{
    public long KickedMemberId { get; init; }
}