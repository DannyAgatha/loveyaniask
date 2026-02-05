using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsEmu.Game.Maps.Event;

namespace NosEmu.Plugins.BasicImplementations.Event.Maps;

public class RemovePortalEventHandler : IAsyncEventProcessor<PortalRemoveEvent>
{
    public async Task HandleAsync(PortalRemoveEvent e, CancellationToken cancellation) => e.Portal.MapInstance.DeletePortal(e.Portal);
}