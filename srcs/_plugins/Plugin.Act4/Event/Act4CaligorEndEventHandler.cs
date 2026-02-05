using PhoenixLib.Events;
using System.Threading;
using System.Threading.Tasks;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Act4.Event;

namespace Plugin.Act4.Event;

public class Act4CaligorEndEventHandler : IAsyncEventProcessor<Act4CaligorEndEvent>
{
    private readonly IAct4CaligorManager _manager;
        
    public Act4CaligorEndEventHandler(IAct4CaligorManager manager)
    {
        _manager = manager;
    }

    public async Task HandleAsync(Act4CaligorEndEvent e, CancellationToken cancellation)
    {
        if (!_manager.CaligorActive)
        {
            return;
        }
            
        _manager.EndCaligorInstance(true);
    }
}