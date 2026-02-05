using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.RainbowBattle.Event;

public class RainbowBattleLeaveEvent : PlayerEvent
{
    public bool SendMessage { get; init; }
    public bool CheckIfFinished { get; init; }
    public bool AddLeaverBuster { get; init; } = true;
}