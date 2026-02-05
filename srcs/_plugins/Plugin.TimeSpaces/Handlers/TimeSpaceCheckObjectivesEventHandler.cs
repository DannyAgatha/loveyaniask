using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Enums;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpaceCheckObjectivesEventHandler : IAsyncEventProcessor<TimeSpaceCheckObjectivesEvent>
{
    private readonly IAsyncEventPipeline _asyncEventPipeline;

    public TimeSpaceCheckObjectivesEventHandler(IAsyncEventPipeline asyncEventPipeline) => _asyncEventPipeline = asyncEventPipeline;

    public async Task HandleAsync(TimeSpaceCheckObjectivesEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        TimeSpaceParty timeSpace = e.TimeSpaceParty;
        TimeSpaceObjective objectives = timeSpace.Instance.TimeSpaceObjective;
        bool playerEnteredToEndPortal = e.PlayerEnteredToEndPortal;

        if (timeSpace.Finished)
        {
            return;
        }

        bool finishTimeSpace = true;

        if (objectives.KillAllMonsters)
        {
            foreach (TimeSpaceSubInstance map in timeSpace.Instance.TimeSpaceSubInstances.Values)
            {
                if (map.MapInstance.GetAliveMonsters(m => m.SummonerType is not VisualType.Player).Count == 0)
                {
                    if (map.SpawnAfterMobsKilled.Count == 0 && map.Task?.MonstersAfterTaskStart.Count == 0)
                    {
                        continue;
                    }
                }

                finishTimeSpace = false;
                break;
            }
        }

        if (objectives.KillMonsterAmount.HasValue)
        {
            finishTimeSpace = objectives.KillMonsterAmount.Value == objectives.KilledMonsterAmount;
        }

        if (objectives.CollectItemAmount.HasValue)
        {
            finishTimeSpace = objectives.CollectItemAmount.Value == objectives.CollectedItemAmount;
        }

        if (objectives.Conversation.HasValue)
        {
            finishTimeSpace = objectives.Conversation.Value == objectives.ConversationsHad;
        }

        if (objectives.InteractObjectsAmount.HasValue)
        {
            finishTimeSpace = objectives.InteractObjectsAmount.Value == objectives.InteractedObjectsAmount;
        }

        if (!finishTimeSpace)
        {
            if (e.SendMessageWithNotFinishedObjects && session != null)
            {
                session.SendChatMessage(session.GetLanguage(GameDialogKey.TIMESPACE_CHATMESSAGE_NOT_FINISHED_TASKS), ChatMessageColorType.PlayerSay);
            }

            return;
        }

        if (objectives.GoToExit && !playerEnteredToEndPortal)
        {
            return;
        }

        if (timeSpace.Instance.PreFinishDialog.HasValue)
        {
            if (timeSpace.Instance.PreFinishDialogTime.HasValue)
            {
                return;
            }

            timeSpace.Instance.PreFinishDialogTime = DateTime.UtcNow;
            return;
        }

        await _asyncEventPipeline.ProcessEventAsync(new TimeSpaceInstanceFinishEvent(timeSpace, TimeSpaceFinishType.SUCCESS));
    }
}