using System;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.RainbowBattle.Event;

public class RainbowBattleFrozenEvent : PlayerEvent
{
    public Guid Id { get; init; }
    public RainbowBattlePlayerDump Killer { get; init; }
    public RainbowBattlePlayerDump Killed { get; init; }
}