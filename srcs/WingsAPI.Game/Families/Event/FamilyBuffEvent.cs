using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Families.Event;

public class FamilyBuffEvent : PlayerEvent
{
    public int ItemVnum { get; init; }
}