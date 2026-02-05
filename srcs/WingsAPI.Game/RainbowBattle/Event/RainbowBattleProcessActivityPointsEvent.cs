using PhoenixLib.Events;

namespace WingsEmu.Game.RainbowBattle.Event;

public class RainbowBattleProcessActivityPointsEvent : IAsyncEvent
{
    public RainbowBattleParty RainbowBattleParty { get; init; }
}