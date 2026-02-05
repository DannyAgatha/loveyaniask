using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Shops.Event;

namespace NosEmu.Plugins.BasicImplementations.CharacterLifetimeStats;

public class TotalGoldSpentInNpcShopEventHandler : IAsyncEventProcessor<ShopNpcBoughtItemEvent>
{
    public async Task HandleAsync(ShopNpcBoughtItemEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        session.PlayerEntity.LifetimeStats.TotalGoldSpentInNpcShop += e.TotalPrice;
        session.PlayerEntity.LifetimeStats.TotalGoldSpent += e.TotalPrice;
    }
}