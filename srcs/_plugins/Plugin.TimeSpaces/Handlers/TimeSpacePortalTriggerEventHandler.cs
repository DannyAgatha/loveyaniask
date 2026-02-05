using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Maps.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpacePortalTriggerEventHandler : IAsyncEventProcessor<PortalTriggerEvent>
{
    private readonly IGameLanguageService _language;
    private readonly IMapManager _mapManager;

    public TimeSpacePortalTriggerEventHandler(IGameLanguageService language, IMapManager mapManager)
    {
        _language = language;
        _mapManager = mapManager;
    }

    public async Task HandleAsync(PortalTriggerEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        if (session.CurrentMapInstance.MapInstanceType != MapInstanceType.TimeSpaceInstance)
        {
            return;
        }

        TimeSpaceParty timeSpaceParty = session.PlayerEntity.TimeSpaceComponent.TimeSpace;
        if (timeSpaceParty?.Instance == null)
        {
            return;
        }

        if (timeSpaceParty.Finished)
        {
            return;
        }

        switch (e.Portal.Type)
        {
            case PortalType.TSNormal:

                if (timeSpaceParty.Started)
                {
                    break;
                }

                if (timeSpaceParty.Leader.PlayerEntity.Id == session.PlayerEntity.Id && !timeSpaceParty.Started)
                {
                    session.SendDialog("rstart 1", "rstart", _language.GetLanguage(GameDialogKey.TIMESPACE_DIALOG_ASK_START, session.UserLanguage));
                    return;
                }
                else if (!timeSpaceParty.Started)
                {
                    session.SendChatMessage(_language.GetLanguage(GameDialogKey.TIMESPACE_CHATMESSAGE_ONLY_TEAM_LEADER_START, session.UserLanguage), ChatMessageColorType.Yellow);
                    return;
                }

                break;
            case PortalType.TSEnd:
                await session.EmitEventAsync(new TimeSpaceCheckObjectivesEvent
                {
                    TimeSpaceParty = session.PlayerEntity.TimeSpaceComponent.TimeSpace,
                    PlayerEnteredToEndPortal = true,
                    SendMessageWithNotFinishedObjects = true
                });
                return;
            case PortalType.Closed:
            case PortalType.TSEndClosed:
                session.SendChatMessage(session.GetLanguage(GameDialogKey.TIMESPACE_CHATMESSAGE_GATE_LOCKED), ChatMessageColorType.PlayerSay);
                return;
        }

        if (e.Portal.DestinationMapInstance == null)
        {
            return;
        }

        e.Sender.PlayerEntity.LastPortal = DateTime.UtcNow;
        if (e.Portal.DestinationX == -1 && e.Portal.DestinationY == -1)
        {
            await _mapManager.TeleportOnRandomPlaceInMapAsync(session, e.Portal.DestinationMapInstance);
            return;
        }

        if (e.Portal.DestinationMapInstance.Id == session.PlayerEntity.MapInstanceId && e.Portal.DestinationX.HasValue && e.Portal.DestinationY.HasValue)
        {
            session.PlayerEntity.TeleportOnMap(e.Portal.DestinationX.Value, e.Portal.DestinationY.Value, true);
            return;
        }

        session.ChangeMap(e.Portal.DestinationMapInstance, e.Portal.DestinationX, e.Portal.DestinationY);
    }
}