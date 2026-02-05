using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsEmu.Game;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpaceStartPortalEventHandler : IAsyncEventProcessor<TimeSpaceStartPortalEvent>
{
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly ITimeSpaceManager _timeSpaceManager;

    public TimeSpaceStartPortalEventHandler(IAsyncEventPipeline eventPipeline, ITimeSpaceManager timeSpaceManager)
    {
        _eventPipeline = eventPipeline;
        _timeSpaceManager = timeSpaceManager;
    }

    public async Task HandleAsync(TimeSpaceStartPortalEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        if (!session.HasCurrentMapInstance || session.CurrentMapInstance.MapInstanceType != MapInstanceType.TimeSpaceInstance)
        {
            return;
        }

        IPortalEntity portal = session.CurrentMapInstance.Portals.FirstOrDefault(s =>
            session.PlayerEntity.PositionY >= s.PositionY - 1 &&
            session.PlayerEntity.PositionY <= s.PositionY + 1 &&
            session.PlayerEntity.PositionX >= s.PositionX - 1 &&
            session.PlayerEntity.PositionX <= s.PositionX + 1);

        if (portal == null)
        {
            Log.Debug("Portal not found");
            return;
        }

        if (portal.DestinationMapInstance == null)
        {
            return;
        }

        if (!session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
        {
            return;
        }

        TimeSpaceParty timeSpace = session.PlayerEntity.TimeSpaceComponent.TimeSpace;
        if (timeSpace == null)
        {
            return;
        }

        if (timeSpace.Leader?.PlayerEntity.Id != session.PlayerEntity.Id)
        {
            return;
        }

        session.PlayerEntity.LastPortal = DateTime.UtcNow;

        if (!timeSpace.Started)
        {
            timeSpace.StartTimeSpace();
            await _eventPipeline.ProcessEventAsync(new TimeSpaceStartClockEvent(session.PlayerEntity.TimeSpaceComponent.TimeSpace, true));
            session.SendPacket(session.CurrentMapInstance.GenerateRsfn(isVisit: true));
        }

        // This is how a normal TS portal should work. Anyways, it can be modified to add portals that TP on the same map, etc...
        session.ChangeMap(portal.DestinationMapInstance, portal.DestinationX, portal.DestinationY);
    }
}