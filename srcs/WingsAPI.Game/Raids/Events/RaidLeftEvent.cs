using System;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Raids.Events;

public class RaidLeftEvent : PlayerEvent
{
    public Guid RaidId { get; init; }
}