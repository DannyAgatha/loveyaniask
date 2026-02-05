using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Maps.Event;

namespace NosEmu.Plugins.BasicImplementations.Event.Maps;

public class DisposeMapEventHandler : IAsyncEventProcessor<DisposeMapEvent>
{
    public Task HandleAsync(DisposeMapEvent e, CancellationToken cancellation)
    {
        e.Map.Destroy();
        return Task.CompletedTask;
    }
}