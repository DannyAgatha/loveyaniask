using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.MapTokenPoints;

public class MapTokenPointsEvent : PlayerEvent
{
    public int ItemVnum { get; init; }
}