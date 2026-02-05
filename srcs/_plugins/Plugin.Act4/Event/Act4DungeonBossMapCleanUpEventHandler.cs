using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;

namespace Plugin.Act4.Event;

public class Act4DungeonBossMapCleanUpEventHandler : IAsyncEventProcessor<Act4DungeonBossMapCleanUpEvent>
{
    public async Task HandleAsync(Act4DungeonBossMapCleanUpEvent e, CancellationToken cancellation)
    {
        foreach (IClientSession session in e.BossMap.MapInstance.Sessions.ToList())
        {
            session.ChangeMap(e.DungeonInstance.SpawnInstance.MapInstance, e.DungeonInstance.SpawnPoint.X, e.DungeonInstance.SpawnPoint.Y);
        }
    }
}