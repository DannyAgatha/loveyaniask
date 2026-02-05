using PhoenixLib.Events;

namespace WingsEmu.Game.TimeSpaces.Events;

public class TimeSpacePortalOpenEvent : IAsyncEvent
{
    public IPortalEntity PortalEntity { get; init; }
}