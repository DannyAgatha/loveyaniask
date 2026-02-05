using PhoenixLib.Events;

namespace WingsEmu.Game.TimeSpaces.Events;

public class TimeSpaceClosePortalEvent : IAsyncEvent
{
    public IPortalEntity PortalEntity { get; init; }
}