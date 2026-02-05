using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.RainbowBattle.Event;

public class RainbowBattleCaptureFlagEvent : PlayerEvent
{
    public INpcEntity NpcEntity { get; init; }
    public bool IsConfirm { get; init; }
}