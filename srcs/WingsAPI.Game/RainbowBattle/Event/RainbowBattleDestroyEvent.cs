using PhoenixLib.Events;

namespace WingsEmu.Game.RainbowBattle.Event;

public class RainbowBattleDestroyEvent : IAsyncEvent
{
    public RainbowBattleParty RainbowBattleParty { get; init; }
}