using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Enums;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpaceInstanceStartEventHandler : IAsyncEventProcessor<TimeSpaceInstanceStartEvent>
{
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IGameLanguageService _gameLanguageService;
    private readonly ITimeSpaceFactory _timeSpaceFactory;
    private readonly ITimeSpaceManager _timeSpaceManager;

    public TimeSpaceInstanceStartEventHandler(IAsyncEventPipeline eventPipeline, ITimeSpaceManager timeSpaceManager, IGameLanguageService gameLanguageService, ITimeSpaceFactory timeSpaceFactory)
    {
        _eventPipeline = eventPipeline;
        _timeSpaceManager = timeSpaceManager;
        _gameLanguageService = gameLanguageService;
        _timeSpaceFactory = timeSpaceFactory;
    }

    public async Task HandleAsync(TimeSpaceInstanceStartEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        TimeSpaceParty timeSpace = session.PlayerEntity.TimeSpaceComponent.TimeSpace;

        if (timeSpace == null || timeSpace.Instance != null)
        {
            return;
        }

        TimeSpaceInstance timeSpaceInstance = _timeSpaceFactory.Create(timeSpace);

        if (timeSpaceInstance == null)
        {
            _timeSpaceManager.RemoveTimeSpace(timeSpace);
            foreach (IClientSession member in timeSpace.Members)
            {
                member.PlayerEntity.TimeSpaceComponent.RemoveTimeSpaceParty();
            }

            return;
        }

        foreach (KeyValuePair<Guid, TimeSpaceSubInstance> timeSpaceSubInstance in timeSpaceInstance.TimeSpaceSubInstances)
        {
            _timeSpaceManager.AddTimeSpaceByMapInstanceId(timeSpaceSubInstance.Value.MapInstance.Id, timeSpace);
            _timeSpaceManager.AddTimeSpaceSubInstance(timeSpaceSubInstance.Key, timeSpaceSubInstance.Value);
        }

        session.PlayerEntity.TimeSpaceComponent.TimeSpace.SetEnteredTimeSpace(timeSpaceInstance);

        IMapInstance mapStart = timeSpaceInstance.SpawnInstance.MapInstance;

        session.ChangeMap(mapStart, timeSpaceInstance.SpawnPoint.X, timeSpaceInstance.SpawnPoint.Y);

        // Time to draw the layout (starting as (0, 0) coordinates the top-left corner). We will need to have a list
        // of mapInstances and iterate over it. Hardcoded for now.
        session.SendPacket(mapStart.GenerateRsfn(true, false));
        foreach (KeyValuePair<Guid, TimeSpaceSubInstance> mapInstance in timeSpaceInstance.TimeSpaceSubInstances)
        {
            session.SendPacket(mapInstance.Value.MapInstance.GenerateRsfn(isVisit: false));
        }

        session.SendRsfpPacket();
        mapStart.MapClear(true);
        mapStart.BroadcastTimeSpacePartnerInfo();

        // Rsfm packet for camera alignment and minimap appearing. The same than before. In the end we have to get the maxX+1 and maxY+1
        // from the MapInstances
        session.SendRsfmPacket(TimeSpaceAction.CAMERA_ADJUST);
        session.SendMinfo();
        session.SendMsg(_gameLanguageService.GetLanguage(GameDialogKey.TIMESPACE_SHOUTMESSAGE_TIMESPACE_ENTER, session.UserLanguage), MsgMessageType.Middle);

        if (timeSpace.IsEasyMode)
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.TIMESPACE_CHATMESSAGE_EASY_MODE), ChatMessageColorType.Yellow);
        }

        if (session.PlayerEntity.Level > timeSpace.HigherLevel)
        {
            timeSpace.HigherLevel = session.PlayerEntity.Level;
        }

        await _eventPipeline.ProcessEventAsync(new TimeSpaceStartClockEvent(session.PlayerEntity.TimeSpaceComponent.TimeSpace, false));
    }
}