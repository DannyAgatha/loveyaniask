using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Bazaar.Events;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.CharacterLifetimeStats;

public class TotalGoldSpentInBazaarItemsEventHandler : IAsyncEventProcessor<BazaarItemBoughtEvent>
{
    public async Task HandleAsync(BazaarItemBoughtEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        session.PlayerEntity.LifetimeStats.TotalGoldSpentInBazaarItems += e.Amount * e.PricePerItem;
        session.PlayerEntity.LifetimeStats.TotalGoldSpent += e.Amount * e.PricePerItem;
    }
}