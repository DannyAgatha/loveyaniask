using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Maps.Event;

public class PortalTriggerEvent : PlayerEvent
{
    public IPortalEntity Portal { get; init; }

    public bool Confirmed { get; init; }
}