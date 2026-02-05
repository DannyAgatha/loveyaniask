using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Revival;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpaceDestroyEventHandler : IAsyncEventProcessor<TimeSpaceDestroyEvent>
{
    private readonly IMapManager _mapManager;
    private readonly ITimeSpaceManager _timeSpaceManager;

    public TimeSpaceDestroyEventHandler(IMapManager mapManager, ITimeSpaceManager timeSpaceManager)
    {
        _mapManager = mapManager;
        _timeSpaceManager = timeSpaceManager;
    }

    public async Task HandleAsync(TimeSpaceDestroyEvent e, CancellationToken cancellation)
    {
        TimeSpaceParty timeSpaceParty = e.TimeSpace;
        _timeSpaceManager.RemoveTimeSpace(timeSpaceParty);
        foreach (TimeSpaceSubInstance subInstance in timeSpaceParty.Instance.TimeSpaceSubInstances.Values)
        {
            Guid mapInstanceId = subInstance.MapInstance.Id;
            _timeSpaceManager.RemoveTimeSpacePartyByMapInstanceId(mapInstanceId);
            _timeSpaceManager.RemoveTimeSpaceSubInstance(mapInstanceId);
            foreach (IClientSession session in subInstance.MapInstance.Sessions)
            {
                session.EmitEvent(new TimeSpaceLeavePartyEvent());
                if (!session.PlayerEntity.IsAlive())
                {
                    await session.EmitEventAsync(new RevivalReviveEvent());
                }

                session.ChangeToLastBaseMap();
            }

            _mapManager.RemoveMapInstance(subInstance.MapInstance.Id);
        }
    }
}