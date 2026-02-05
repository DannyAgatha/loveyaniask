using System;
using System.Collections.Generic;
using System.Linq;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Ship.Configuration;

namespace WingsEmu.Game.Ship;

public class ShipInstance
{
    public ShipInstance(IMapInstance mapInstance, Configuration.Ship configuration, DateTime currentTime)
    {
        MapInstance = mapInstance;
        Configuration = configuration;
        DepartureWarnings = Configuration.DepartureWarnings.ToList();
        LastDeparture = currentTime;
    }

    public ShipType ShipType => Configuration.ShipType;
    public IMapInstance MapInstance { get; }
    public Configuration.Ship Configuration { get; }
    public List<TimeSpan> DepartureWarnings { get; set; }
    public DateTime LastDeparture { get; set; }
}