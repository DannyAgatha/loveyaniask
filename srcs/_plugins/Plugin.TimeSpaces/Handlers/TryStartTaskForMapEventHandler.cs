using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;

namespace Plugin.TimeSpaces.Handlers;

public class TryStartTaskForMapEventHandler : IAsyncEventProcessor<TryStartTaskEvent>
{
    private readonly IAsyncEventPipeline _asyncEventPipeline;

    public TryStartTaskForMapEventHandler(IAsyncEventPipeline asyncEventPipeline) => _asyncEventPipeline = asyncEventPipeline;

    public async Task HandleAsync(TryStartTaskEvent e, CancellationToken cancellation)
    {
        TimeSpaceSubInstance map = e.Map;
        if (map.Task == null)
        {
            return;
        }

        if (map.Task.IsActivated)
        {
            return;
        }

        if (map.Task.IsFinished)
        {
            return;
        }

        if (map.Task.StartDialog.HasValue && map.Task.DialogStartTask)
        {
            return;
        }

        await _asyncEventPipeline.ProcessEventAsync(new TimeSpaceStartTaskEvent
        {
            TimeSpaceSubInstance = map
        }, cancellation);
    }
}