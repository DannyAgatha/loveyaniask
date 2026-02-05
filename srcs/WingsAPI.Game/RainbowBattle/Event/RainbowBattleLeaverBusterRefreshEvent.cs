using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.RainbowBattle.Event;

public class RainbowBattleLeaverBusterRefreshEvent : PlayerEvent
{
    public bool Force { get; init; }
}