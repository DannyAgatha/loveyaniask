using System.Threading.Tasks;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Inventory;

public class WearPacketHandler : GenericGamePacketHandlerBase<WearPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, WearPacket wearPacket)
    {
        if (session.PlayerEntity.IsSeal)
        {
            return;
        }

        if (wearPacket.PetId == 0)
        {
            await session.EmitEventAsync(new InventoryEquipItemEvent(wearPacket.InventorySlot, boundItem: wearPacket.BoundItem));
            return;
        }

        await session.EmitEventAsync(new PartnerInventoryEquipItemEvent(wearPacket.PetId, wearPacket.InventorySlot));
    }
}