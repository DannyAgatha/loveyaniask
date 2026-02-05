using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsEmu.Game;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Maps;

namespace Plugin.Act4.Event;

public class Act4DungeonSystemStopEventHandler : IAsyncEventProcessor<Act4DungeonSystemStopEvent>
{
    private readonly IAct4DungeonManager _act4DungeonManager;
    private readonly IAsyncEventPipeline _asyncEventPipeline;
    private readonly IMapManager _mapManager;

    public Act4DungeonSystemStopEventHandler(IAct4DungeonManager act4DungeonManager, IAsyncEventPipeline asyncEventPipeline, IMapManager mapManager)
    {
        _act4DungeonManager = act4DungeonManager;
        _asyncEventPipeline = asyncEventPipeline;
        _mapManager = mapManager;
    }

    public async Task HandleAsync(Act4DungeonSystemStopEvent e, CancellationToken cancellation)
    {
        if (!_act4DungeonManager.DungeonsActive)
        {
            return;
        }

        (IReadOnlyList<IMonsterEntity> guardiansToRemove, IPortalEntity portal) = _act4DungeonManager.GetAndCleanGuardians();

        portal.MapInstance.DeletePortal(portal);

        foreach (IMonsterEntity guardian in guardiansToRemove)
        {
            await guardian.EmitEventAsync(new MapLeaveMonsterEntityEvent(guardian));
        }

        _act4DungeonManager.DisableDungeons();

        await _asyncEventPipeline.ProcessEventAsync(new Act4SystemFcBroadcastEvent(), cancellation);

        IReadOnlyList<DungeonInstance> dungeons = _act4DungeonManager.Dungeons;
        if (dungeons.Count < 1)
        {
            return;
        }

        foreach (DungeonInstance dungeon in dungeons.ToList())
        {
            await _asyncEventPipeline.ProcessEventAsync(new Act4DungeonStopEvent
            {
                DungeonInstance = dungeon
            }, cancellation);
        }
    }
}