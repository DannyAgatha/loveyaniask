using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Managers;

namespace Plugin.Act4.Event;

public class Act4FactionPointsIncreaseEventHandler : IAsyncEventProcessor<Act4FactionPointsIncreaseEvent>
{
    private readonly IAct4Manager _act4Manager;

    public Act4FactionPointsIncreaseEventHandler(IAct4Manager act4Manager) => _act4Manager = act4Manager;

    public async Task HandleAsync(Act4FactionPointsIncreaseEvent e, CancellationToken cancellation)
    {
        if (_act4Manager.FactionPointsLocked)
        {
            return;
        }

        _act4Manager.AddFactionPoints(e.FactionType, e.PointsToAdd);
    }
}