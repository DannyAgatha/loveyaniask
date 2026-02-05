using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Characters;

namespace WingsEmu.Game.RainbowBattle.Event;

public class RainbowBattleUnfreezeEvent : PlayerEvent
{
    public IPlayerEntity Unfreezer { get; init; }
}