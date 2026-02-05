using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Families.Event;

public class FamilyDisbandedEvent : PlayerEvent
{
    public long FamilyId { get; init; }
}