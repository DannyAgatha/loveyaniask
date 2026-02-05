using System;
using PhoenixLib.Events;

namespace WingsEmu.Game.Ship.Event;

public class ShipProcessEvent : IAsyncEvent
{
    public ShipProcessEvent(ShipInstance shipInstance, DateTime currentTime)
    {
        ShipInstance = shipInstance;
        CurrentTime = currentTime;
    }

    public ShipInstance ShipInstance { get; }
    public DateTime CurrentTime { get; }
}