using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Enums;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.Enums;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpaceStartTaskEventHandler : IAsyncEventProcessor<TimeSpaceStartTaskEvent>
{
    private readonly IAsyncEventPipeline _asyncEventPipeline;
    private readonly IRandomGenerator _randomGenerator;

    public TimeSpaceStartTaskEventHandler(IAsyncEventPipeline asyncEventPipeline, IRandomGenerator randomGenerator)
    {
        _asyncEventPipeline = asyncEventPipeline;
        _randomGenerator = randomGenerator;
    }

    public async Task HandleAsync(TimeSpaceStartTaskEvent e, CancellationToken cancellation)
    {
        TimeSpaceSubInstance room = e.TimeSpaceSubInstance;
        if (room.Task == null)
        {
            return;
        }

        if (room.Task.IsFinished)
        {
            return;
        }

        room.TimeSpaceWave ??= DateTime.UtcNow;
        if (room.Task.IsActivated)
        {
            SendWarnings(room);
            return;
        }

        TimeSpaceTask task = room.Task;
        DateTime now = DateTime.UtcNow;

        if (!task.IsActivated && task.TimeLeft == null && task.Time.HasValue)
        {
            task.TimeLeft = now + task.Time.Value;
        }

        SendWarnings(room);
        task.IsActivated = true;
        task.TaskStart = now;
        room.MapInstance.AIDisabled = false;

        foreach (IMonsterEntity monster in room.MapInstance.GetAliveMonsters())
        {
            monster.NextTick = now + TimeSpan.FromMilliseconds(_randomGenerator.RandomNumber(1000));
        }

        foreach (INpcEntity npc in room.MapInstance.GetAliveNpcs())
        {
            npc.NextTick = now + TimeSpan.FromMilliseconds(_randomGenerator.RandomNumber(1000));
        }

        if (!room.Task.MonstersAfterTaskStart.Any())
        {
            return;
        }

        var toRemove = new List<(int?, IMonsterEntity)>();
        foreach ((int? neededKills, IMonsterEntity monster) in room.Task.MonstersAfterTaskStart)
        {
            if (neededKills.HasValue)
            {
                continue;
            }

            await monster.EmitEventAsync(new MapJoinMonsterEntityEvent(monster, monster.Position.X, monster.Position.Y));
            toRemove.Add((null, monster));
        }

        foreach ((int? kills, IMonsterEntity monster) in toRemove)
        {
            room.Task.MonstersAfterTaskStart.Remove((kills, monster));
        }

        await _asyncEventPipeline.ProcessEventAsync(new TimeSpaceBonusMonsterEvent
        {
            TimeSpaceSubInstance = room,
            MonsterEntities = room.MapInstance.GetAliveMonsters()
        });
    }

    private void SendWarnings(TimeSpaceSubInstance timeSpaceSubInstance)
    {
        TimeSpaceTask task = timeSpaceSubInstance.Task;
        if (task.TimeLeft.HasValue && task.Time.HasValue)
        {
            TimeSpan timeLeft = task.TimeLeft.Value - DateTime.UtcNow;
            if (timeLeft.TotalMilliseconds > 0)
            {
                foreach (IClientSession sessionOnMap in timeSpaceSubInstance.MapInstance.Sessions)
                {
                    sessionOnMap.SendClockPacket(ClockType.RedMiddle, 0, timeLeft, task.Time.Value);
                }
            }
        }

        if (timeSpaceSubInstance.Task.TaskType != TimeSpaceTaskType.None)
        {
            foreach (IClientSession sessionOnMap in timeSpaceSubInstance.MapInstance.Sessions)
            {
                sessionOnMap.SendRsfmPacket(!timeSpaceSubInstance.Task.IsActivated ? TimeSpaceAction.WARNING_WITH_SOUND : TimeSpaceAction.WARNING_WITHOUT_SOUND);
            }
        }

        if (string.IsNullOrEmpty(task.GameDialogKey))
        {
            return;
        }

        foreach (IClientSession sessionOnMap in timeSpaceSubInstance.MapInstance.Sessions)
        {
            string message = sessionOnMap.GetLanguage(task.GameDialogKey);
            sessionOnMap.SendMissionTargetMessage(message);
        }
    }
}