using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsEmu.Game;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.Enums;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpaceClosePortalEventHandler : IAsyncEventProcessor<TimeSpaceClosePortalEvent>
{
    public async Task HandleAsync(TimeSpaceClosePortalEvent e, CancellationToken cancellation)
    {
        IPortalEntity portal = e.PortalEntity;

        PortalType type = portal.Type switch
        {
            PortalType.Open => PortalType.Closed,
            PortalType.TSEnd => PortalType.TSEndClosed,
            _ => PortalType.Closed
        };

        portal.Type = type;
        portal.MapInstance.MapClear(true);
        portal.MapInstance.BroadcastTimeSpacePartnerInfo();
    }
}