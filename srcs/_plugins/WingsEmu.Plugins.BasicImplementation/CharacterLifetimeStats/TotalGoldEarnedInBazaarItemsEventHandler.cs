using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Bazaar.Events;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.CharacterLifetimeStats;

public class TotalGoldEarnedInBazaarItemsEventHandler : IAsyncEventProcessor<BazaarItemWithdrawnEvent>
{
    public async Task HandleAsync(BazaarItemWithdrawnEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        session.PlayerEntity.LifetimeStats.TotalGoldEarnedInBazaarItems += e.Price * e.Quantity;
    }
}