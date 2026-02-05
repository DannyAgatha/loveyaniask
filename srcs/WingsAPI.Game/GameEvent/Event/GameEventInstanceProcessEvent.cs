using System;
using PhoenixLib.Events;

namespace WingsEmu.Game.GameEvent.Event;

public class GameEventInstanceProcessEvent : IAsyncEvent
{
    public GameEventInstanceProcessEvent(DateTime currentTime) => CurrentTime = currentTime;

    public DateTime CurrentTime { get; }
}