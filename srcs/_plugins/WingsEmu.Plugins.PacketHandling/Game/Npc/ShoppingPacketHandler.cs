using System.Threading.Tasks;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Shops.Event;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Npc;

public class ShoppingPacketHandler : GenericGamePacketHandlerBase<ShoppingPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, ShoppingPacket packet)
    {
        if (session.PlayerEntity.IsShopping || !session.HasCurrentMapInstance)
        {
            return;
        }

        byte type = packet.Type;
        int npcId = packet.NpcId;

        INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(npcId);
        if (npcEntity?.ShopNpc == null)
        {
            return;
        }

        session.EmitEvent(new ShopNpcListItemsEvent
        {
            NpcId = npcId,
            ShopType = type
        });
    }
}