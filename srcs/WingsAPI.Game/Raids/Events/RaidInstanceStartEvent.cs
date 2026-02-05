// NosEmu
// 


using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Raids.Events;

public class RaidInstanceStartEvent : PlayerEvent
{
    public bool ForceTeleport { get; init; }
}