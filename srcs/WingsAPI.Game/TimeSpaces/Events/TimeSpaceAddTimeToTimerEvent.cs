using System;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.TimeSpaces.Events;

public class TimeSpaceAddTimeToTimerEvent : PlayerEvent
{
    public TimeSpaceParty TimeSpaceParty { get; init; }
    public TimeSpan Time { get; init; }
}