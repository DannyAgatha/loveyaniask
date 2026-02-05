using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Families;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Shops;
using WingsEmu.Game.Shops.Event;

namespace NosEmu.Plugins.BasicImplementations.Shop;

public class BuyItemNpcShopEventHandler : IAsyncEventProcessor<BuyItemNpcShopEvent>
{
    public async Task HandleAsync(BuyItemNpcShopEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        int amount = e.Amount;
        short slot = e.Slot;
        long ownerId = e.OwnerId;

        // load shop

        INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(ownerId);
        if (npcEntity == null)
        {
            return;
        }

        int dist = session.PlayerEntity.GetDistance(npcEntity);
        if (npcEntity.ShopNpc == null)
        {
            return;
        }

        if (dist > 5)
        {
            return;
        }

        switch (npcEntity.ShopNpc.MenuType)
        {
            case ShopNpcMenuType.ITEMS:
            case ShopNpcMenuType.MINILAND:
                await session.EmitEventAsync(new BuyShopItemEvent
                {
                    Amount = amount,
                    OwnerId = ownerId,
                    Slot = slot
                });
                break;
            case ShopNpcMenuType.SKILLS:
                await session.EmitEventAsync(new BuyShopSkillEvent
                {
                    OwnerId = ownerId,
                    Slot = slot,
                    Accept = e.Accept
                });
                break;
            case ShopNpcMenuType.FAMILIES:
                await session.EmitEventAsync(new FamilyUpgradeBuyFromShopEvent
                {
                    NpcId = ownerId,
                    Slot = slot
                });
                break;
        }
    }
}