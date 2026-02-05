using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.InterChannel;

public class LandOfLifeMessageEvent: PlayerEvent
{
    public string Message { get; init; }
}