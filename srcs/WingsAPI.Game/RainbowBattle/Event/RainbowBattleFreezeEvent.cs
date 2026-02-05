using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.RainbowBattle.Event;

public class RainbowBattleFreezeEvent : PlayerEvent
{
    public IBattleEntity Killer { get; init; }
}