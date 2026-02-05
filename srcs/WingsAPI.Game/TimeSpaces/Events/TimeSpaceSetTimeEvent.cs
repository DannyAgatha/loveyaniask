using System;
using PhoenixLib.Events;

namespace WingsEmu.Game.TimeSpaces.Events;

public class TimeSpaceSetTimeEvent : IAsyncEvent
{
    public TimeSpaceParty TimeSpaceParty { get; init; }
    public TimeSpan Time { get; init; }
}