using System;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.RainbowBattle.Event;

public class RainbowBattleWonEvent : PlayerEvent
{
    public Guid Id { get; init; }
    public int[] Players { get; init; }
}