using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsEmu.Game.Maps.Event;

namespace NosEmu.Plugins.BasicImplementations.Event.Maps;

public class SpawnPortalEventHandler : IAsyncEventProcessor<SpawnPortalEvent>
{
    public async Task HandleAsync(SpawnPortalEvent e, CancellationToken cancellation) => e.Map.AddPortalToMap(e.Portal);
}