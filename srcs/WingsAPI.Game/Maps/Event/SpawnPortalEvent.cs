using PhoenixLib.Events;

namespace WingsEmu.Game.Maps.Event;

public class SpawnPortalEvent : IAsyncEvent
{
    public SpawnPortalEvent(IMapInstance map, IPortalEntity portal)
    {
        Map = map;
        Portal = portal;
    }

    public IMapInstance Map { get; }
    public IPortalEntity Portal { get; }
}