using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._enum;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Maps.Event;
using WingsEmu.Game.Networking;

namespace Plugin.Act4.Event;

public class Act4JoinMapEndEventHandler : IAsyncEventProcessor<JoinMapEndEvent>
{
    private readonly IAct4DungeonManager _act4DungeonManager;
    private readonly IAct4Manager _act4Manager;

    public Act4JoinMapEndEventHandler(IAct4Manager act4Manager, IAct4DungeonManager act4DungeonManager)
    {
        _act4Manager = act4Manager;
        _act4DungeonManager = act4DungeonManager;
    }

    public async Task HandleAsync(JoinMapEndEvent e, CancellationToken cancellation)
    {
        e.Sender.SendFcPacket(_act4Manager.GetStatus());
        e.Sender.SendGuriFactionOverridePacket();

        if (e.JoinedMapInstance.MapInstanceType != MapInstanceType.Act4Dungeon || e.Sender.PlayerEntity.Family == null)
        {
            return;
        }

        DungeonInstance dungeon = _act4DungeonManager.GetDungeon(e.Sender.PlayerEntity.Family.Id);

        if (dungeon == null || !dungeon.DungeonSubInstances.TryGetValue(e.JoinedMapInstance.Id, out DungeonSubInstance dungeonSubInstance))
        {
            return;
        }

        bool firstPlayerToJoinInBoss = (dungeonSubInstance.LastDungeonWaveLoop == null || dungeonSubInstance.LastDungeonWaveLinear == null || dungeonSubInstance.LastPortalGeneration == null) &&
            dungeonSubInstance.Bosses.Count > 0;

        DateTime currentTime = DateTime.UtcNow;
        e.Sender.SendDungeonPacket(dungeon, dungeonSubInstance, _act4DungeonManager, currentTime);
        dungeonSubInstance.LastDungeonWaveLoop ??= currentTime;
        dungeonSubInstance.LastDungeonWaveLinear ??= currentTime;
        dungeonSubInstance.LastPortalGeneration ??= currentTime;

        if (!firstPlayerToJoinInBoss)
        {
            return;
        }

        dungeon.StartInBoosRoom = currentTime;
        foreach (DungeonSubInstance subInstance in dungeon.DungeonSubInstances.Values)
        {
            foreach (DungeonLoopWave wave in subInstance.LoopWaves)
            {
                wave.FirstSpawnWave = currentTime + wave.Delay;
            }

            if (subInstance == dungeonSubInstance)
            {
                continue;
            }

            foreach (IClientSession session in subInstance.MapInstance.Sessions)
            {
                session.ChangeMap(dungeonSubInstance.MapInstance, e.Sender.PlayerEntity.PositionX, e.Sender.PlayerEntity.PositionY);
            }
        }
    }
}