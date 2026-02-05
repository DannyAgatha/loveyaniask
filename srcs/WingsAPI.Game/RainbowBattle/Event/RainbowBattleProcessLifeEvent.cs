using PhoenixLib.Events;

namespace WingsEmu.Game.RainbowBattle.Event;

public class RainbowBattleProcessLifeEvent : IAsyncEvent
{
    public RainbowBattleParty RainbowBattleParty { get; init; }
}