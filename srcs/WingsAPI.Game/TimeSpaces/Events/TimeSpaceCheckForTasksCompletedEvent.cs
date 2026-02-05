using System.Collections.Generic;
using PhoenixLib.Events;

namespace WingsEmu.Game.TimeSpaces.Events;

public class TimeSpaceCheckForTasksCompletedEvent : TimeSpaceTaskCheckEvent
{
    public IEnumerable<TimeSpaceSubInstance> TimeSpaceSubInstances { get; init; }
    public IEnumerable<IAsyncEvent> Events { get; init; }
}