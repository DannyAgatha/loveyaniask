using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Scripting.Object.Timespace;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Maps.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Enums;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpaceJoinMapEndEventHandler : IAsyncEventProcessor<JoinMapEndEvent>
{
    private readonly ITimeSpaceManager _timeSpaceManager;

    public TimeSpaceJoinMapEndEventHandler(ITimeSpaceManager timeSpaceManager) => _timeSpaceManager = timeSpaceManager;

    public async Task HandleAsync(JoinMapEndEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        if (e.JoinedMapInstance.MapInstanceType != MapInstanceType.TimeSpaceInstance)
        {
            return;
        }

        if (!session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
        {
            return;
        }

        TimeSpaceParty timeSpace = session.PlayerEntity.TimeSpaceComponent.TimeSpace;

        if (timeSpace?.Instance == null)
        {
            return;
        }

        if (timeSpace.Instance.StartTimeFreeze.HasValue)
        {
            await session.EmitEventAsync(new TimeSpaceAddTimeToTimerEvent
            {
                Time = DateTime.UtcNow - timeSpace.Instance.StartTimeFreeze.Value
            });

            timeSpace.Instance.StartTimeFreeze = null;
        }

        session.RefreshTimespaceScoreUi();
        if (!timeSpace.Instance.VisitedRooms.Contains(e.JoinedMapInstance.Id) && timeSpace.Instance.SpawnInstance.MapInstance.Id != e.JoinedMapInstance.Id)
        {
            timeSpace.Instance.IncreaseEnteredRooms();
            timeSpace.Instance.VisitedRooms.Add(e.JoinedMapInstance.Id);
        }

        if (!timeSpace.Instance.TimeSpaceSubInstances.TryGetValue(e.JoinedMapInstance.Id, out TimeSpaceSubInstance room))
        {
            return;
        }

        if (room.SendPortalOpenMessage)
        {
            room.SendPortalOpenMessage = false;
            session.SendMsg(session.GetLanguage(GameDialogKey.TIMESPACE_SHOUTMESSAGE_DOOR_OPENED), MsgMessageType.Middle);
        }

        if (!string.IsNullOrEmpty(room.Task.StartDialogShout))
        {
            session.SendMsg(session.GetLanguage(room.Task.StartDialogShout), MsgMessageType.Middle);
        }

        if (room.Task?.StartDialog != null)
        {
            session.SendNpcReqPacket(room.Task.StartDialog.Value);

            if (timeSpace.Instance.StartTimeFreeze.HasValue)
            {
                return;
            }

            if (!timeSpace.Started || timeSpace.Finished)
            {
                return;
            }

            timeSpace.Instance.StartTimeFreeze = DateTime.UtcNow;
            foreach (IClientSession member in timeSpace.Members)
            {
                member.SendTimerFreeze();
            }
        }

        SendWarnings(session, room);
        await room.TriggerEvents(TimespaceConstEventKeys.OnMapJoin);
    }

    private void SendWarnings(IClientSession session, TimeSpaceSubInstance timeSpaceSubInstance)
    {
        TimeSpaceTask task = timeSpaceSubInstance.Task;
        if (task == null)
        {
            return;
        }

        if (task.IsFinished || !task.IsActivated)
        {
            return;
        }

        if (task.TimeLeft.HasValue && task.Time.HasValue)
        {
            TimeSpan timeLeft = task.TimeLeft.Value - DateTime.UtcNow;
            if (timeLeft.TotalMilliseconds > 0)
            {
                session.SendClockPacket(ClockType.RedMiddle, 0, timeLeft, task.Time.Value);
            }
        }

        if (timeSpaceSubInstance.Task.TaskType != TimeSpaceTaskType.None)
        {
            session.SendRsfmPacket(!timeSpaceSubInstance.Task.IsActivated ? TimeSpaceAction.WARNING_WITH_SOUND : TimeSpaceAction.WARNING_WITHOUT_SOUND);
        }

        if (string.IsNullOrEmpty(task.GameDialogKey))
        {
            return;
        }

        string message = session.GetLanguage(task.GameDialogKey);
        session.SendMissionTargetMessage(message);
    }
}