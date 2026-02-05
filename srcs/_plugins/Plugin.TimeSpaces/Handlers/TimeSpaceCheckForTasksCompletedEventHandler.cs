using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpaceCheckForTasksCompletedEventHandler : IAsyncEventProcessor<TimeSpaceCheckForTasksCompletedEvent>
{
    private readonly IAsyncEventPipeline _asyncEventPipeline;

    public TimeSpaceCheckForTasksCompletedEventHandler(IAsyncEventPipeline asyncEventPipeline) => _asyncEventPipeline = asyncEventPipeline;

    public async Task HandleAsync(TimeSpaceCheckForTasksCompletedEvent e, CancellationToken cancellation)
    {
        if (e.Completed)
        {
            return;
        }

        IEnumerable<TimeSpaceSubInstance> timeSpaceSubInstances = e.TimeSpaceSubInstances;
        IEnumerable<IAsyncEvent> events = e.Events;

        bool isFinished = true;

        foreach (TimeSpaceSubInstance timeSpaceSubInstance in timeSpaceSubInstances)
        {
            if (timeSpaceSubInstance.Task == null)
            {
                continue;
            }

            if (timeSpaceSubInstance.Task.IsFinished)
            {
                continue;
            }

            isFinished = false;
            break;
        }

        if (!isFinished)
        {
            return;
        }

        e.Completed = true;
        foreach (IAsyncEvent asyncEvent in events)
        {
            await _asyncEventPipeline.ProcessEventAsync(asyncEvent);
        }
    }
}