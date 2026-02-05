using System.Collections.Generic;
using WingsEmu.Game.Ship;
using WingsEmu.Game.Ship.Configuration;

namespace NosEmu.Plugins.BasicImplementations.Ship;

public class ShipManager : IShipManager
{
    private readonly List<ShipInstance> _ships = new();
    private readonly Dictionary<ShipType, ShipInstance> _shipsByShipType = new();

    public void AddShip(ShipInstance shipInstance)
    {
        if (_shipsByShipType.TryAdd(shipInstance.ShipType, shipInstance))
        {
            _ships.Add(shipInstance);
        }
    }

    public ShipInstance GetShip(ShipType shipType) => _shipsByShipType.TryGetValue(shipType, out ShipInstance shipInstance) ? shipInstance : null;

    public IReadOnlyCollection<ShipInstance> GetShips() => _ships;
}