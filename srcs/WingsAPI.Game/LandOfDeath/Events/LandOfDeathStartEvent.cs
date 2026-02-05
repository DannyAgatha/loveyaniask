using System;
using PhoenixLib.Events;

namespace WingsEmu.Game.LandOfDeath.Events;

public class LandOfDeathStartEvent : IAsyncEvent
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}
