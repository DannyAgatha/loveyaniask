using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game;

public class PlayerCommandEvent : PlayerEvent
{
    public string Command { get; init; }
}