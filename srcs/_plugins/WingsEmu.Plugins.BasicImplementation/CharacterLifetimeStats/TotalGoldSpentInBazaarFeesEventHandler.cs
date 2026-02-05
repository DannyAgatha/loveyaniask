using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Bazaar.Events;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.CharacterLifetimeStats;

public class TotalGoldSpentInBazaarFeesEventHandler : IAsyncEventProcessor<BazaarItemAddedEvent>
{
    public async Task HandleAsync(BazaarItemAddedEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        session.PlayerEntity.LifetimeStats.TotalGoldSpentInBazaarFees += e.Tax;
        session.PlayerEntity.LifetimeStats.TotalGoldSpent += e.Tax;
    }
}