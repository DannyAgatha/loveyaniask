using PhoenixLib.Events;

namespace WingsEmu.Game.RainbowBattle.Event;

public class RainbowBattleUnfreezeProcessEvent : IAsyncEvent
{
    public RainbowBattleParty RainbowBattleParty { get; init; }
}