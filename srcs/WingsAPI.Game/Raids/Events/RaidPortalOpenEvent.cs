using PhoenixLib.Events;

namespace WingsEmu.Game.Raids.Events;

public class RaidPortalOpenEvent : IAsyncEvent
{
    public RaidPortalOpenEvent(RaidSubInstance raidSubInstance, IPortalEntity portal)
    {
        RaidSubInstance = raidSubInstance;
        Portal = portal;
    }

    public RaidSubInstance RaidSubInstance { get; }

    public IPortalEntity Portal { get; }
}