using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Families.Event;

public class FamilyLeftEvent : PlayerEvent
{
    public long FamilyId { get; init; }
}