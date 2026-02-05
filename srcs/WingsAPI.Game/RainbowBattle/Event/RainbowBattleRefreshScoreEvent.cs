using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.RainbowBattle.Event;

public class RainbowBattleRefreshScoreEvent : PlayerEvent
{
    public RainbowBattleParty RainbowBattleParty { get; init; }
}