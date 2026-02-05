using System;
using System.Collections.Generic;
using WingsEmu.Core;

namespace WingsEmu.Game.Ship.Configuration;

public class ShipConfiguration : List<Ship>
{
}

public class Ship
{
    public ShipType ShipType { get; set; }
    public long ShipCost { get; set; }
    public byte ShipLevelRestriction { get; set; }

    public short ShipMapId { get; set; }
    public Range<short> ShipMapX { get; set; }
    public Range<short> ShipMapY { get; set; }

    public TimeSpan Departure { get; set; }
    public List<TimeSpan> DepartureWarnings { get; set; }

    public int DestinationMapId { get; set; }
    public Range<short> DestinationMapX { get; set; }
    public Range<short> DestinationMapY { get; set; }
}

public enum ShipType
{
    Act4Angels,
    Act4Demons,
    Act5,
    Act7
}