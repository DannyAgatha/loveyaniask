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

public class TimeSpacePortalOpenEventHandler : IAsyncEventProcessor<TimeSpacePortalOpenEvent>
{
    private readonly ITimeSpaceManager _timeSpaceManager;

    public TimeSpacePortalOpenEventHandler(ITimeSpaceManager timeSpaceManager) => _timeSpaceManager = timeSpaceManager;

    public async Task HandleAsync(TimeSpacePortalOpenEvent e, CancellationToken cancellation)
    {
        IPortalEntity portal = e.PortalEntity;

        PortalType type = portal.Type switch
        {
            PortalType.Closed => PortalType.Open,
            PortalType.TSEndClosed => PortalType.TSEnd,
            _ => PortalType.Open
        };

        portal.Type = type;

        portal.MapInstance.MapClear(true);
        portal.MapInstance.BroadcastTimeSpacePartnerInfo();

        TimeSpaceSubInstance timeSpaceSubInstance = _timeSpaceManager.GetSubInstance(portal.MapInstance.Id);

        if (timeSpaceSubInstance == null)
        {
            return;
        }

        bool sendClock = false;
        if (timeSpaceSubInstance.Task != null)
        {
            if (timeSpaceSubInstance.Task.IsFinished && timeSpaceSubInstance.Task.TimeLeft.HasValue)
            {
                sendClock = true;
                timeSpaceSubInstance.Task.TimeLeft = null;
            }
        }

        if (!portal.MapInstance.Sessions.Any())
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

            if (!sendClock)
            {
                continue;
            }

            session.SendRemoveRedClockPacket();
        }
    }
}