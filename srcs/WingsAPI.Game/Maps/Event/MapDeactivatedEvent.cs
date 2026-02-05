using PhoenixLib.Events;

namespace WingsEmu.Game.Maps.Event;

public class MapDeactivatedEvent : IAsyncEvent
{
    public MapDeactivatedEvent(IMapInstance mapInstance) => MapInstance = mapInstance;

    public IMapInstance MapInstance { get; }
}