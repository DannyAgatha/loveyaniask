using PhoenixLib.Events;

namespace WingsEmu.Game.RainbowBattle.Event;

public class RainbowBattleProcessFlagPointsEvent : IAsyncEvent
{
    public RainbowBattleParty RainbowBattleParty { get; init; }
}