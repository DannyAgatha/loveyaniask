using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpaceCheckMonsterEventHandler : IAsyncEventProcessor<TimeSpaceCheckMonsterEvent>
{
    private readonly ITimeSpaceManager _timeSpaceManager;

    public TimeSpaceCheckMonsterEventHandler(ITimeSpaceManager timeSpaceManager) => _timeSpaceManager = timeSpaceManager;

    public async Task HandleAsync(TimeSpaceCheckMonsterEvent e, CancellationToken cancellation)
    {
        Guid guid = e.MonsterEntity.MapInstance.Id;

        TimeSpaceSubInstance timeSpaceSubInstance = _timeSpaceManager.GetSubInstance(guid);
        if (timeSpaceSubInstance == null)
        {
            return;
        }

        await CheckTask(timeSpaceSubInstance);
        if (timeSpaceSubInstance.SpawnAfterMobsKilled.Count < 1)
        {
            return;
        }

        long killedMonsters = timeSpaceSubInstance.MapInstance.MonsterDeathsOnMap();
        HashSet<int> toRemove = new();
        foreach ((int toSpawn, List<IMonsterEntity> monsters) in timeSpaceSubInstance.SpawnAfterMobsKilled)
        {
            if (monsters == null)
            {
                continue;
            }

            if (toSpawn > killedMonsters)
            {
                continue;
            }

            foreach (IMonsterEntity monster in monsters)
            {
                await monster.EmitEventAsync(new MapJoinMonsterEntityEvent(monster, monster.Position.X, monster.Position.Y, true));
            }

            toRemove.Add(toSpawn);
        }

        foreach (int toSpawn in toRemove)
        {
            timeSpaceSubInstance.SpawnAfterMobsKilled.Remove(toSpawn);
        }
    }

    private async Task CheckTask(TimeSpaceSubInstance instance)
    {
        TimeSpaceTask task = instance.Task;

        if (task == null)
        {
            return;
        }

        if (task.IsFinished)
        {
            return;
        }

        if (!task.IsActivated)
        {
            return;
        }

        if (task.MonstersAfterTaskStart.Count < 1 || task.MonstersAfterTaskStart.All(x => x.Item1 == null))
        {
            return;
        }

        long killedMonsters = instance.MapInstance.MonsterDeathsOnMap();

        var toRemove = new List<(int?, IMonsterEntity)>();
        foreach ((int? toKill, IMonsterEntity monster) in task.MonstersAfterTaskStart)
        {
            if (!toKill.HasValue)
            {
                continue;
            }

            if (toKill.Value > killedMonsters)
            {
                continue;
            }

            await monster.EmitEventAsync(new MapJoinMonsterEntityEvent(monster, monster.Position.X, monster.Position.Y, true));
            toRemove.Add((toKill.Value, monster));
        }

        foreach ((int? toKill, IMonsterEntity monster) in toRemove)
        {
            task.MonstersAfterTaskStart.Remove((toKill, monster));
        }
    }
}