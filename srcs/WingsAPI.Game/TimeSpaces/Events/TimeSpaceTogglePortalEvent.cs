using PhoenixLib.Events;

namespace WingsEmu.Game.TimeSpaces.Events;

public class TimeSpaceTogglePortalEvent : IAsyncEvent
{
    public IPortalEntity PortalEntity { get; init; }
}