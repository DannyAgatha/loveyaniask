using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpaceTogglePortalEventHandler : IAsyncEventProcessor<TimeSpaceTogglePortalEvent>
{
    private readonly ITimeSpaceManager _timeSpaceManager;

    public TimeSpaceTogglePortalEventHandler(ITimeSpaceManager timeSpaceManager) => _timeSpaceManager = timeSpaceManager;

    public async Task HandleAsync(TimeSpaceTogglePortalEvent e, CancellationToken cancellation)
    {
        IPortalEntity portal = e.PortalEntity;
        TimeSpaceSubInstance timeSpaceSubInstance = _timeSpaceManager.GetSubInstance(portal.MapInstance.Id);

        if (timeSpaceSubInstance == null)
        {
            return;
        }

        PortalType type = portal.Type switch
        {
            PortalType.Closed => PortalType.Open,
            PortalType.Open => PortalType.Closed,
            PortalType.TSEndClosed => PortalType.TSEnd,
            PortalType.TSEnd => PortalType.TSEndClosed,
            _ => PortalType.Open
        };

        portal.Type = type;
        timeSpaceSubInstance.MapInstance.MapClear(true);
        timeSpaceSubInstance.MapInstance.BroadcastTimeSpacePartnerInfo();

        if (type is PortalType.Closed or PortalType.TSEndClosed)
        {
            return;
        }

        if (!timeSpaceSubInstance.MapInstance.Sessions.Any())
        {
            timeSpaceSubInstance.SendPortalOpenMessage = true;

            TimeSpaceParty timeSpace = _timeSpaceManager.GetTimeSpaceByMapInstanceId(portal.MapInstance.Id);
            if (timeSpace == null)
            {
                return;
            }

            foreach (IClientSession session in timeSpace.Members.Where(session => session.CurrentMapInstance.Id != portal.MapInstance.Id))
            {
                session.SendMsg(session.GetLanguage(GameDialogKey.TIMESPACE_SHOUTMESSAGE_DOOR_OPENED_SOMEWHERE), MsgMessageType.Middle);
            }

            return;
        }

        foreach (IClientSession session in portal.MapInstance.Sessions)
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.TIMESPACE_SHOUTMESSAGE_DOOR_OPENED), MsgMessageType.Middle);
        }
    }
}