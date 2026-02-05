using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Act4.Configuration;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Revival;

namespace Plugin.Act4.Event;

public class Act4DungeonStopEventHandler : IAsyncEventProcessor<Act4DungeonStopEvent>
{
    private readonly IAct4DungeonManager _act4DungeonManager;
    private readonly Act4DungeonsConfiguration _act4DungeonsConfiguration;
    private readonly IMapManager _mapManager;

    public Act4DungeonStopEventHandler(IAct4DungeonManager act4DungeonManager, IMapManager mapManager, Act4DungeonsConfiguration act4DungeonsConfiguration)
    {
        _act4DungeonManager = act4DungeonManager;
        _mapManager = mapManager;
        _act4DungeonsConfiguration = act4DungeonsConfiguration;
    }

    public async Task HandleAsync(Act4DungeonStopEvent e, CancellationToken cancellation)
    {
        _act4DungeonManager.UnregisterDungeon(e.DungeonInstance);

        foreach (DungeonSubInstance subInstance in e.DungeonInstance.DungeonSubInstances.Values)
        {
            foreach (IClientSession session in subInstance.MapInstance.Sessions.ToList())
            {
                if (!session.PlayerEntity.IsAlive())
                {
                    await session.EmitEventAsync(new RevivalReviveEvent());
                }

                session.ChangeMap(_act4DungeonsConfiguration.DungeonReturnPortalMapId, _act4DungeonsConfiguration.DungeonReturnPortalMapX, _act4DungeonsConfiguration.DungeonReturnPortalMapY);
            }

            _mapManager.RemoveMapInstance(subInstance.MapInstance.Id);
            subInstance.MapInstance.Destroy();
        }
    }
}