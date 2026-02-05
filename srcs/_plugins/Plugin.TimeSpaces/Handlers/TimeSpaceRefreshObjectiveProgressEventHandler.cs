using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpaceRefreshObjectiveProgressEventHandler : IAsyncEventProcessor<TimeSpaceRefreshObjectiveProgressEvent>
{
    private readonly ITimeSpaceManager _timeSpaceManager;

    public TimeSpaceRefreshObjectiveProgressEventHandler(ITimeSpaceManager timeSpaceManager) => _timeSpaceManager = timeSpaceManager;

    public async Task HandleAsync(TimeSpaceRefreshObjectiveProgressEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        Guid mapInstanceId = e.MapInstanceId;

        TimeSpaceParty timeSpace = session == null ? _timeSpaceManager.GetTimeSpaceByMapInstanceId(mapInstanceId) : session.PlayerEntity.TimeSpaceComponent.TimeSpace;
        if (timeSpace == null)
        {
            return;
        }

        foreach (IClientSession member in timeSpace.Members)
        {
            member.SendMsg(member.GetLanguage(GameDialogKey.TIMESPACE_SHOUTMESSAGE_MISSION_UPDATED), MsgMessageType.Middle);
            member.SendMinfo();
        }
    }
}