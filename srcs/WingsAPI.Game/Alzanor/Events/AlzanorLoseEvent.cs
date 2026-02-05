using System;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Alzanor.Events;

public class AlzanorLoseEvent : PlayerEvent
{
    public Guid Id { get; init; }
    public int[] Players { get; init; }
}