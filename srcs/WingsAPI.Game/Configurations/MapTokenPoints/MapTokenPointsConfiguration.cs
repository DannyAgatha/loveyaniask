using System.Collections.Generic;

namespace WingsEmu.Game.Configurations.MapTokenPoints;

public class MapTokenPointsConfiguration
{
    public List<MapTokenPoint> MapTokenPoints { get; init; } = [];
}

public class MapTokenPoint
{
    public int MapId { get; init; }
    public TokenPoint TokenPointA { get; init; }
    public TokenPoint TokenPointB { get; init; }
    public TokenPoint TokenPointC { get; init; }
}

public class TokenPoint
{
    public short X { get; init; }
    public short Y { get; init; }
    public byte Direction { get; init; }
}