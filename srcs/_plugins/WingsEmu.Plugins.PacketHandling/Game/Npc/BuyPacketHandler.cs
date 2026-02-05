using System.Threading.Tasks;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Shops.Event;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.PacketHandling.Game.Npc;

public class BuyPacketHandler : GenericGamePacketHandlerBase<BuyPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, BuyPacket buyPacket)
    {
        if (session.PlayerEntity.IsInExchange())
        {
            return;
        }

        if (!session.HasCurrentMapInstance)
        {
            return;
        }

        switch (buyPacket.Type)
        {
            case BuyShopType.CharacterShop:
                await session.EmitEventAsync(new ShopPlayerBuyItemEvent
                {
                    OwnerId = buyPacket.OwnerId,
                    Amount = buyPacket.Amount,
                    Slot = buyPacket.Slot
                });
                break;

            case BuyShopType.ItemShop:
                await session.EmitEventAsync(new BuyItemNpcShopEvent
                {
                    OwnerId = buyPacket.OwnerId,
                    Amount = buyPacket.Amount,
                    Slot = buyPacket.Slot,
                    Accept = buyPacket.Amount == 1
                });
                break;

            default:
                return;
        }
    }
}