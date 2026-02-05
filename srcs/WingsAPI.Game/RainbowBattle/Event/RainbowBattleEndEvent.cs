using PhoenixLib.Events;

namespace WingsEmu.Game.RainbowBattle.Event;

public class RainbowBattleEndEvent : IAsyncEvent
{
    public RainbowBattleParty RainbowBattleParty { get; init; }
}