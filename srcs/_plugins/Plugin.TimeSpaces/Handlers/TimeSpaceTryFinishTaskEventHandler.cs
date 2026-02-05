using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Scripting.Object.Timespace;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Enums;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpaceTryFinishTaskEventHandler : IAsyncEventProcessor<TimeSpaceTryFinishTaskEvent>
{
    private readonly IAsyncEventPipeline _asyncEventPipeline;

    public TimeSpaceTryFinishTaskEventHandler(IAsyncEventPipeline asyncEventPipeline) => _asyncEventPipeline = asyncEventPipeline;

    public async Task HandleAsync(TimeSpaceTryFinishTaskEvent e, CancellationToken cancellation)
    {
        TimeSpaceSubInstance room = e.TimeSpaceSubInstance;
        TimeSpaceTask task = room.Task;
        TimeSpaceParty timeSpaceParty = e.TimeSpaceParty;

        if (task.TaskStart.AddSeconds(1) > DateTime.UtcNow && task.TaskType != TimeSpaceTaskType.None)
        {
            return;
        }

        switch (task.TaskType)
        {
            case TimeSpaceTaskType.None:
                room.Task.IsFinished = true;
                await room.TriggerEvents(TimespaceConstEventKeys.OnTaskFinish);
                BroadcastEndDialog(room, timeSpaceParty);
                return;
            case TimeSpaceTaskType.KillAllMonsters:

                bool noMonsterToSpawn = room.SpawnAfterMobsKilled.Count == 0 && task.MonstersAfterTaskStart.Count == 0;
                int monsters = room.MapInstance.GetAliveMonsters(m => m.SummonerType is not VisualType.Player).Count;

                if (task.Time == null || task.TimeLeft == null)
                {
                    if (monsters > 0 || !noMonsterToSpawn)
                    {
                        return;
                    }

                    room.Task.IsFinished = true;
                    await room.TriggerEvents(TimespaceConstEventKeys.OnTaskFinish);
                    BroadcastEndDialog(room, timeSpaceParty);
                    await IncreaseScore(timeSpaceParty);
                    room.MapInstance.Broadcast(x => x.GenerateRemoveRedClock());
                    return;
                }

                if (monsters <= 0 && noMonsterToSpawn)
                {
                    room.Task.IsFinished = true;
                    await room.TriggerEvents(TimespaceConstEventKeys.OnTaskFinish);
                    BroadcastEndDialog(room, timeSpaceParty);
                    await IncreaseScore(timeSpaceParty);
                    room.MapInstance.Broadcast(x => x.GenerateRemoveRedClock());
                    return;
                }

                TimeSpan timeLeft = task.TimeLeft.Value - DateTime.UtcNow;
                if (timeLeft.TotalMilliseconds > 0)
                {
                    return;
                }

                room.Task.IsFinished = true;
                await room.TriggerEvents(TimespaceConstEventKeys.OnTaskFail);
                room.MapInstance.Broadcast(x => x.GenerateRemoveRedClock());

                break;
            case TimeSpaceTaskType.Survive:
                if (task.Time == null || task.TimeLeft == null)
                {
                    room.Task.IsFinished = true;
                    await room.TriggerEvents(TimespaceConstEventKeys.OnTaskFinish);
                    BroadcastEndDialog(room, timeSpaceParty);
                    await IncreaseScore(timeSpaceParty);
                    room.MapInstance.Broadcast(x => x.GenerateRemoveRedClock());
                    return;
                }

                timeLeft = task.TimeLeft.Value - DateTime.UtcNow;
                if (timeLeft.TotalMilliseconds > 0)
                {
                    return;
                }

                room.Task.IsFinished = true;
                await room.TriggerEvents(TimespaceConstEventKeys.OnTaskFinish);
                BroadcastEndDialog(room, timeSpaceParty);
                await IncreaseScore(timeSpaceParty);
                room.MapInstance.Broadcast(x => x.GenerateRemoveRedClock());
                break;
        }
    }

    private async Task IncreaseScore(TimeSpaceParty timeSpaceParty)
    {
        await _asyncEventPipeline.ProcessEventAsync(new TimeSpaceIncreaseScoreEvent
        {
            TimeSpaceParty = timeSpaceParty,
            AmountToIncrease = 50
        });
    }

    private void BroadcastEndDialog(TimeSpaceSubInstance timeSpaceSubInstance, TimeSpaceParty timeSpace)
    {
        foreach (IClientSession session in timeSpaceSubInstance.MapInstance.Sessions)
        {
            session.SendMissionTargetMessage(string.Empty);
            if (timeSpaceSubInstance.Task.TaskType != TimeSpaceTaskType.None)
            {
                session.SendMsg(session.GetLanguage(GameDialogKey.TIMESPACE_SHOUTMESSAGE_QUICK_MISSION_DONE), MsgMessageType.BottomCard);
            }
        }

        TimeSpaceTask task = timeSpaceSubInstance.Task;
        if (task.EndDialog.HasValue)
        {
            foreach (IClientSession session in timeSpaceSubInstance.MapInstance.Sessions)
            {
                session.SendNpcReqPacket(task.EndDialog.Value);
            }

            timeSpace.Instance.StartTimeFreeze = DateTime.UtcNow;
            foreach (IClientSession member in timeSpace.Members)
            {
                member.SendTimerFreeze();
            }
        }

        if (string.IsNullOrEmpty(task.EndDialogShout))
        {
            return;
        }

        foreach (IClientSession session in timeSpaceSubInstance.MapInstance.Sessions)
        {
            session.SendMsg(session.GetLanguage(task.EndDialogShout), MsgMessageType.Middle);
        }
    }
}