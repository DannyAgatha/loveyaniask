using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.RainbowBattle.Event;

public class RainbowBattleTieEvent : PlayerEvent
{
    public int[] RedTeam { get; init; }
    public int[] BlueTeam { get; init; }
}