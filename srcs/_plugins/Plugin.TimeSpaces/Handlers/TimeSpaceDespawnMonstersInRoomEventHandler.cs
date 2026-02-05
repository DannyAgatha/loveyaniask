using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.Enums;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpaceDespawnMonstersInRoomEventHandler : IAsyncEventProcessor<TimeSpaceDespawnMonstersInRoomEvent>
{
    private readonly IAsyncEventPipeline _asyncEventPipeline;

    public TimeSpaceDespawnMonstersInRoomEventHandler(IAsyncEventPipeline asyncEventPipeline) => _asyncEventPipeline = asyncEventPipeline;

    public async Task HandleAsync(TimeSpaceDespawnMonstersInRoomEvent e, CancellationToken cancellation)
    {
        TimeSpaceSubInstance timeSpaceSubInstance = e.TimeSpaceSubInstance;

        if (timeSpaceSubInstance == null)
        {
            return;
        }

        timeSpaceSubInstance.Task?.MonstersAfterTaskStart.Clear();
        timeSpaceSubInstance.SpawnAfterMobsKilled.Clear();

        foreach (IMonsterEntity monster in timeSpaceSubInstance.MapInstance.GetAliveMonsters(x => x.SummonerType is not VisualType.Player))
        {
            monster.MapInstance.Broadcast(monster.GenerateOut());
            await _asyncEventPipeline.ProcessEventAsync(new MonsterDeathEvent(monster)
            {
                IsByCommand = true
            });
        }
    }
}