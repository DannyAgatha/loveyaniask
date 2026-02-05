using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Act4.Event;

public class Act4DungeonEnterEvent : PlayerEvent
{
    public bool Confirmed { get; init; }
}