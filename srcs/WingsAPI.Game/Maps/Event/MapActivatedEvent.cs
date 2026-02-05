using PhoenixLib.Events;

namespace WingsEmu.Game.Maps.Event;

public class MapActivatedEvent : IAsyncEvent
{
    public MapActivatedEvent(IMapInstance mapInstance) => MapInstance = mapInstance;

    public IMapInstance MapInstance { get; }
}