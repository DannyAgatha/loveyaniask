using PhoenixLib.Events;

namespace WingsEmu.Game.Maps.Event;

public class PortalRemoveEvent : IAsyncEvent
{
   public PortalRemoveEvent(IPortalEntity portalEntity)
    {
        Portal = portalEntity;
    }
    public IPortalEntity Portal { get; init; }
}