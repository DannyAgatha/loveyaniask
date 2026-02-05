using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Ship.Configuration;

namespace WingsEmu.Game.Ship.Event;

public class ShipEnterEvent : PlayerEvent
{
    public ShipEnterEvent(ShipType shipType) => ShipType = shipType;

    public ShipType ShipType { get; }
}