using System;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.TimeSpaces.Events;

public class TimeSpaceRefreshObjectiveProgressEvent : PlayerEvent
{
    public Guid MapInstanceId { get; init; }
}