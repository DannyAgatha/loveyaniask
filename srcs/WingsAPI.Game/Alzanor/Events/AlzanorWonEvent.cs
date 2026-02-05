using System;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Alzanor.Events;

public class AlzanorWonEvent : PlayerEvent
{
    public Guid Id { get; init; }
    public int[] Players { get; init; }
}