using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Maps;

namespace NosEmu.Plugins.BasicImplementations.Entities;

public class MapLeaveMonsterEntityEventHandler : IAsyncEventProcessor<MapLeaveMonsterEntityEvent>
{
    public async Task HandleAsync(MapLeaveMonsterEntityEvent e, CancellationToken cancellation)
    {
        IMapInstance mapInstance = e.MonsterEntity.MapInstance;
        if (mapInstance == null)
        {
            return;
        }

        mapInstance.Broadcast(e.MonsterEntity.GenerateOut());
        mapInstance.RemoveMonster(e.MonsterEntity);
    }
}