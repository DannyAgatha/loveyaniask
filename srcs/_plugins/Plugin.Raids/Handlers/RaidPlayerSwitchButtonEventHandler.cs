using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using Plugin.Raids.Const;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Raids.Handlers;

public class RaidPlayerSwitchButtonEventHandler : IAsyncEventProcessor<RaidPlayerSwitchButtonEvent>
{
    public async Task HandleAsync(RaidPlayerSwitchButtonEvent e, CancellationToken cancellation)
    {
        e.ButtonMapItem.State = !e.ButtonMapItem.State;
        e.ButtonMapItem.ItemVNum = e.ButtonMapItem.State ? e.ButtonMapItem.ActivatedStateVNum : e.ButtonMapItem.DeactivatedStateVNum;
        await CheckTimeSpaceMessage(e.Sender, e.ButtonMapItem);
        await e.ButtonMapItem.TriggerEvents(RaidConstEventKeys.ButtonSwitched);

        if (e.ButtonMapItem.NonDefaultState)
        {
            await e.ButtonMapItem.TriggerEvents(RaidConstEventKeys.ButtonTriggered);
        }

        if (e.ButtonMapItem.CanBeMovedOnlyOnce.HasValue && !e.ButtonMapItem.CanBeMovedOnlyOnce.Value)
        {
            e.ButtonMapItem.CanBeMovedOnlyOnce = true;
        }

        e.ButtonMapItem.BroadcastIn();
        await e.Sender.EmitEventAsync(new RaidSwitchButtonToggledEvent
        {
            LeverId = e.ButtonMapItem.TransportId
        });
    }

    private async Task CheckTimeSpaceMessage(IClientSession session, ButtonMapItem button)
    {
        TimeSpaceParty timeSpace = session.PlayerEntity.TimeSpaceComponent.TimeSpace;
        if (timeSpace?.Instance == null)
        {
            return;
        }

        foreach (IClientSession member in timeSpace.Members)
        {
            member.SendMsg(member.GetLanguage(GameDialogKey.TIMESPACE_SHOUTMESSAGE_LEVER_OPERATED), MsgMessageType.Middle);
        }

        if (!timeSpace.Instance.TimeSpaceObjective.InteractObjectsVnum.HasValue)
        {
            return;
        }

        // The lever has been already moved, so it using deactivated vnum
        if (timeSpace.Instance.TimeSpaceObjective.InteractObjectsVnum.Value != button.DeactivatedStateVNum)
        {
            return;
        }

        if (!button.IsObjective)
        {
            return;
        }

        if (button.AlreadyMoved)
        {
            return;
        }

        button.AlreadyMoved = true;
        timeSpace.Instance.TimeSpaceObjective.InteractedObjectsAmount++;
        await session.EmitEventAsync(new TimeSpaceRefreshObjectiveProgressEvent());
    }
}