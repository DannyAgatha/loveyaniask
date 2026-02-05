using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Characters.Events;

public class SpUntransformEvent : PlayerEvent
{
    public bool Force { get; set; }
}