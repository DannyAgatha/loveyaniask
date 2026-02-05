using System.Collections.Generic;
using WingsEmu.Game._enum;

namespace WingsEmu.Game.Configurations.Miniland;

public class Miniland
{
    public SerializablePosition ArrivalSerializablePosition = new() { X = 5, Y = 8 };

    public int DefaultMaximumCapacity = 10;
    public int MapVnum { get; set; } = (int)MapIds.MINILAND;

    public int MapItemVnum { get; set; } = (int)ItemVnums.MINILAND_THEME_SPRING;

    public List<ForcedPlacing> ForcedPlacings { get; set; } = new();

    public List<RestrictedZone> RestrictedZones { get; set; } = new();
}