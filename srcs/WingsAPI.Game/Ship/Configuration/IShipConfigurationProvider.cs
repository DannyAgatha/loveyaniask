using System.Collections.Generic;

namespace WingsEmu.Game.Ship.Configuration;

public interface IShipConfigurationProvider
{
    IReadOnlyList<Ship> GetShips();
}