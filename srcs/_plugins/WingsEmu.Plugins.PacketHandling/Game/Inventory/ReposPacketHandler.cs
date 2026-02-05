using System.Threading.Tasks;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Warehouse.Events;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Inventory;

public class ReposPacketHandler : GenericGamePacketHandlerBase<ReposPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, ReposPacket reposPacket)
    {
        if (reposPacket.IsPartnerBackpack)
        {
            await session.EmitEventAsync(new PartnerWarehouseMoveEvent(reposPacket.OldSlot, reposPacket.Amount, reposPacket.NewSlot));
            return;
        }

        await session.EmitEventAsync(new AccountWarehouseMoveEvent(reposPacket.OldSlot, reposPacket.Amount, reposPacket.NewSlot));
    }
}