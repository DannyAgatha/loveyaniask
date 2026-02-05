using System.Collections.Generic;
using WingsEmu.Game.Ship.Configuration;

namespace WingsEmu.Game.Ship;

public interface IShipManager
{
    void AddShip(ShipInstance shipInstance);
    ShipInstance GetShip(ShipType shipType);
    IReadOnlyCollection<ShipInstance> GetShips();
}