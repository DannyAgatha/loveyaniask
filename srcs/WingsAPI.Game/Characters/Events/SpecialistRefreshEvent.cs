using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Characters.Events;

public class SpecialistRefreshEvent : PlayerEvent
{
    public bool Force { get; set; }
}