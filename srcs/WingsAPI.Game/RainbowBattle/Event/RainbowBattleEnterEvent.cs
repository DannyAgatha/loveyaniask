using PhoenixLib.Events;

namespace WingsEmu.Game.RainbowBattle.Event;

public class RainbowBattleEnterEvent : IAsyncEvent
{
    public long[] Players { get; init; }
}