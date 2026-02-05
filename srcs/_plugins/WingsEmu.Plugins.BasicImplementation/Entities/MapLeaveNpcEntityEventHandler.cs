using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Maps;

namespace NosEmu.Plugins.BasicImplementations.Entities;

public class MapLeaveNpcEntityEventHandler : IAsyncEventProcessor<MapLeaveNpcEntityEvent>
{
    public async Task HandleAsync(MapLeaveNpcEntityEvent e, CancellationToken cancellation)
    {
        IMapInstance mapInstance = e.NpcEntity.MapInstance;
        if (mapInstance == null)
        {
            return;
        }

        mapInstance.Broadcast(e.NpcEntity.GenerateOut());
        mapInstance.RemoveNpc(e.NpcEntity);
    }
}