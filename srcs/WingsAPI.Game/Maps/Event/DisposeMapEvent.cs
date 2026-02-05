using PhoenixLib.Events;

namespace WingsEmu.Game.Maps.Event;

public class DisposeMapEvent : IAsyncEvent
{
    public DisposeMapEvent(IMapInstance map) => Map = map;

    public IMapInstance Map { get; }
}