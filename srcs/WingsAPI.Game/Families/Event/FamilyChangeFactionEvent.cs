using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Families.Event;

public class FamilyChangeFactionEvent : PlayerEvent
{
    public int Faction { get; init; }
}