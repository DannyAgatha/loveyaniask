using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Enums;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.TimeSpaces.RecurrentJob;

public class TimeSpaceSystem : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly ITimeSpaceManager _timeSpaceManager;

    public TimeSpaceSystem(IAsyncEventPipeline eventPipeline, ITimeSpaceManager timeSpaceManager)
    {
        _eventPipeline = eventPipeline;
        _timeSpaceManager = timeSpaceManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Info("[TIMESPACE_SYSTEM] Started!");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                foreach (TimeSpaceParty timeSpace in _timeSpaceManager.GetTimeSpaces())
                {
                    await ProcessTimeSpace(timeSpace);
                }
            }
            catch (Exception e)
            {
                Log.Error("[TIMESPACE_SYSTEM]", e);
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessTimeSpace(TimeSpaceParty timeSpaceParty)
    {
        if (timeSpaceParty.Instance == null)
        {
            return;
        }

        DateTime currentDate = DateTime.UtcNow;
        await TryFinish(timeSpaceParty, currentDate);
        await TryRemove(timeSpaceParty, currentDate);
        await ProcessObjectivesCheck(timeSpaceParty, currentDate);
        await ProcessPreDialog(timeSpaceParty, currentDate);
        foreach (TimeSpaceSubInstance room in timeSpaceParty.Instance.TimeSpaceSubInstances.Values)
        {
            await TryFinishRoomTask(timeSpaceParty, room, currentDate);
            await CheckBonusMonster(room);
            await ProcessMonsterWave(room, currentDate);
        }
    }

    private async Task ProcessMonsterWave(TimeSpaceSubInstance room, DateTime currentDate)
    {
        TimeSpaceWave wave = room.TimeSpaceWaves.FirstOrDefault();
        if (wave == null || room.TimeSpaceWave == null || currentDate < room.TimeSpaceWave + wave.Delay)
        {
            return;
        }

        room.TimeSpaceWaves.Remove(wave);
        foreach (IClientSession session in room.MapInstance.Sessions)
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.TIMESPACE_SHOUTMESSAGE_ENEMIES_REINFORCEMENTS), MsgMessageType.Middle);
        }

        await _eventPipeline.ProcessEventAsync(new MonsterSummonEvent(room.MapInstance, wave.Monsters, showEffect: true));
    }

    private async Task ProcessPreDialog(TimeSpaceParty timeSpaceParty, DateTime currentDate)
    {
        if (!timeSpaceParty.Instance.PreFinishDialog.HasValue)
        {
            return;
        }

        if (timeSpaceParty.Instance.PreFinishDialogTime == null)
        {
            return;
        }

        if (timeSpaceParty.Instance.PreFinishDialogTime > currentDate)
        {
            return;
        }

        if (timeSpaceParty.Instance.PreFinishDialogShown)
        {
            return;
        }

        timeSpaceParty.Instance.PreFinishDialogShown = true;
        foreach (IClientSession member in timeSpaceParty.Members)
        {
            member.SendNpcReqPacket(timeSpaceParty.Instance.PreFinishDialog.Value);
        }
    }

    private async Task ProcessObjectivesCheck(TimeSpaceParty timeSpaceParty, DateTime dateTime)
    {
        if (!timeSpaceParty.Started || timeSpaceParty.Finished || timeSpaceParty.Destroy)
        {
            return;
        }

        if (timeSpaceParty.LastObjectivesCheck > dateTime)
        {
            return;
        }

        await _eventPipeline.ProcessEventAsync(new TimeSpaceCheckObjectivesEvent
        {
            TimeSpaceParty = timeSpaceParty
        });
    }

    private async Task CheckBonusMonster(TimeSpaceSubInstance room)
    {
        if (room.MonsterBonusId.HasValue)
        {
            return;
        }

        await _eventPipeline.ProcessEventAsync(new TimeSpaceBonusMonsterEvent
        {
            MonsterEntities = room.MapInstance.GetAliveMonsters(),
            TimeSpaceSubInstance = room
        });
    }

    private async Task TryFinishRoomTask(TimeSpaceParty timeSpaceParty, TimeSpaceSubInstance room, DateTime dateTime)
    {
        if (timeSpaceParty.Finished)
        {
            return;
        }

        if (room.MapInstance.Sessions.Count < 1)
        {
            return;
        }

        if (room.Task == null)
        {
            return;
        }

        if (!room.Task.IsActivated)
        {
            return;
        }

        if (room.Task.IsFinished)
        {
            return;
        }

        if (room.LastTryFinishTime > dateTime)
        {
            return;
        }

        await _eventPipeline.ProcessEventAsync(new TimeSpaceTryFinishTaskEvent(room, timeSpaceParty));
    }

    private async Task TryRemove(TimeSpaceParty timeSpaceParty, DateTime currentDate)
    {
        if (timeSpaceParty.Members.Count == 0)
        {
            Log.Warn("[TIMESPACE_SYSTEM] Destroying Time-Space instance");
            await _eventPipeline.ProcessEventAsync(new TimeSpaceDestroyEvent(timeSpaceParty));
            return;
        }

        if (!timeSpaceParty.Finished)
        {
            return;
        }

        if (!timeSpaceParty.Started)
        {
            return;
        }

        if (timeSpaceParty.Instance.RemoveDate > currentDate)
        {
            return;
        }

        Log.Warn("[TIMESPACE_SYSTEM] Destroying Time-Space instance");
        await _eventPipeline.ProcessEventAsync(new TimeSpaceDestroyEvent(timeSpaceParty));
    }

    private async Task TryFinish(TimeSpaceParty timeSpaceParty, DateTime currentTime)
    {
        if (!timeSpaceParty.Started || timeSpaceParty.Finished || timeSpaceParty.Instance.FinishDate > currentTime || timeSpaceParty.Instance.StartTimeFreeze.HasValue
            || timeSpaceParty.Instance.InfiniteDuration)
        {
            return;
        }

        await _eventPipeline.ProcessEventAsync(new TimeSpaceInstanceFinishEvent(timeSpaceParty, TimeSpaceFinishType.TIME_IS_UP));
    }
}