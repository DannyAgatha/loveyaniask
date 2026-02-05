using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;

namespace Plugin.Act4.Event;

public class Act4DungeonBroadcastPacketEventHandler : IAsyncEventProcessor<Act4DungeonBroadcastPacketEvent>
{
    private readonly IAct4DungeonManager _act4DungeonManager;

    public Act4DungeonBroadcastPacketEventHandler(IAct4DungeonManager act4DungeonManager) => _act4DungeonManager = act4DungeonManager;

    public async Task HandleAsync(Act4DungeonBroadcastPacketEvent e, CancellationToken cancellation)
    {
        IReadOnlyDictionary<Guid, DungeonSubInstance> subInstances = e.DungeonInstance.DungeonSubInstances;
        if (subInstances.Count < 1)
        {
            return;
        }

        DateTime currentTime = DateTime.UtcNow;

        foreach (DungeonSubInstance subInstance in subInstances.Values)
        {
            string packet = null;

            foreach (IClientSession session in subInstance.MapInstance.Sessions)
            {
                packet ??= session.GenerateDungeonPacket(e.DungeonInstance, subInstance, _act4DungeonManager, currentTime);
                session.SendPacket(packet);
            }
        }
    }
}