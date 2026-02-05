using System;
using System.Threading.Tasks;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.TimeSpace;

public class TimeSpaceDialogHandler : INpcDialogAsyncHandler
{
    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.TIME_SPACE_END_DIALOG };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        if (!session.HasCurrentMapInstance)
        {
            return;
        }

        if (!session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
        {
            return;
        }

        if (session.CurrentMapInstance.MapInstanceType != MapInstanceType.TimeSpaceInstance)
        {
            return;
        }

        TimeSpaceParty timeSpace = session.PlayerEntity.TimeSpaceComponent.TimeSpace;
        if (timeSpace?.Instance == null)
        {
            return;
        }

        if (!timeSpace.Instance.TimeSpaceSubInstances.TryGetValue(session.CurrentMapInstance.Id, out TimeSpaceSubInstance instance))
        {
            return;
        }

        if (instance.Task == null)
        {
            return;
        }

        if (instance.Task.StartDialog.HasValue)
        {
            if (!instance.Task.DialogStartTask)
            {
                await TryAddFrozenTime(session, timeSpace);
                return;
            }

            instance.Task.StartDialog = null;
            await TryAddFrozenTime(session, timeSpace);

            if (instance.Task.StartDialogIsObjective)
            {
                instance.Task.StartDialogIsObjective = false;
                timeSpace.Instance.TimeSpaceObjective.ConversationsHad++;
                await session.EmitEventAsync(new TimeSpaceRefreshObjectiveProgressEvent());
            }

            if (instance.Task.IsFinished)
            {
                return;
            }

            await session.EmitEventAsync(new TimeSpaceStartTaskEvent
            {
                TimeSpaceSubInstance = instance
            });
            return;
        }

        if (!instance.Task.EndDialog.HasValue)
        {
            return;
        }

        instance.Task.EndDialog = null;
        await TryAddFrozenTime(session, timeSpace);

        if (!instance.Task.EndDialogIsObjective)
        {
            return;
        }

        if (instance.Task.IsFinished)
        {
            return;
        }

        instance.Task.EndDialogIsObjective = false;
        timeSpace.Instance.TimeSpaceObjective.ConversationsHad++;
        await session.EmitEventAsync(new TimeSpaceRefreshObjectiveProgressEvent());
    }

    private async Task TryAddFrozenTime(IClientSession session, TimeSpaceParty timeSpaceParty)
    {
        if (!timeSpaceParty.Instance.StartTimeFreeze.HasValue)
        {
            return;
        }

        await session.EmitEventAsync(new TimeSpaceAddTimeToTimerEvent
        {
            Time = DateTime.UtcNow - timeSpaceParty.Instance.StartTimeFreeze.Value
        });

        timeSpaceParty.Instance.StartTimeFreeze = null;
    }
}