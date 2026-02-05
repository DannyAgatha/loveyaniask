using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Ship.Event;

namespace NosEmu.Plugins.BasicImplementations.Ship.Event;

public class ShipLeaveEventHandler : IAsyncEventProcessor<ShipLeaveEvent>
{
    public async Task HandleAsync(ShipLeaveEvent e, CancellationToken cancellation) => e.Sender.ChangeToLastBaseMap();
}