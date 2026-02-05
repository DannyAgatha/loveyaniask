using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Packets.Enums;

namespace Plugin.Raids;

public class RaidPortalOpenEventHandler : IAsyncEventProcessor<RaidPortalOpenEvent>
{
    public async Task HandleAsync(RaidPortalOpenEvent e, CancellationToken cancellation)
    {
        e.Portal.Type = PortalType.Open;

        e.RaidSubInstance.MapInstance.MapClear();
    }
}