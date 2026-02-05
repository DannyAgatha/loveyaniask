using System.Collections.Generic;
using System.Linq;
using WingsEmu.Game.Ship.Configuration;

namespace NosEmu.Plugins.BasicImplementations.Ship;

public class ShipConfigurationProvider : IShipConfigurationProvider
{
    private readonly IReadOnlyList<WingsEmu.Game.Ship.Configuration.Ship> _ships;

    public ShipConfigurationProvider(ShipConfiguration shipConfiguration) => _ships = PrepareConfiguration(shipConfiguration);

    public IReadOnlyList<WingsEmu.Game.Ship.Configuration.Ship> GetShips() => _ships;

    private static List<WingsEmu.Game.Ship.Configuration.Ship> PrepareConfiguration(ShipConfiguration shipConfiguration)
    {
        var list = shipConfiguration.ToList();
        foreach (WingsEmu.Game.Ship.Configuration.Ship ship in list)
        {
            ship.DepartureWarnings = ship.DepartureWarnings.OrderBy(w => w).ToList();
        }

        return list;
    }
}