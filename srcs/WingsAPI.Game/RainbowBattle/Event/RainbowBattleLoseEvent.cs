using System;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.RainbowBattle.Event;

public class RainbowBattleLoseEvent : PlayerEvent
{
    public Guid Id { get; init; }
    public int[] Players { get; init; }
}