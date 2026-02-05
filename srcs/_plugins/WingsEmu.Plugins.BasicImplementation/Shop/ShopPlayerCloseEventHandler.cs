using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Shops.Event;

namespace NosEmu.Plugins.BasicImplementations.Shop;

public class ShopPlayerCloseEventHandler : IAsyncEventProcessor<ShopPlayerCloseEvent>
{
    public async Task HandleAsync(ShopPlayerCloseEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        if (!session.PlayerEntity.HasShopOpened || !session.HasCurrentMapInstance)
        {
            return;
        }

        session.PlayerEntity.ShopComponent.RemoveShop();
        session.PlayerEntity.HasShopOpened = false;
        session.PlayerEntity.IsShopping = false;
        session.BroadcastShop();
        session.BroadcastPlayerShopFlag(0);
        session.SendCondPacket();

        await session.EmitEventAsync(new PlayerRestEvent
        {
            RestTeamMemberMates = false
        });

        await session.EmitEventAsync(new ShopClosedEvent());
    }
}