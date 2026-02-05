using System.Threading.Tasks;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Warehouse.Events;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Inventory;

public class WithdrawPacketHandler : GenericGamePacketHandlerBase<WithdrawPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, WithdrawPacket withdrawPacket)
    {
        if (withdrawPacket.PetBackpack)
        {
            await session.EmitEventAsync(new PartnerWarehouseWithdrawEvent(withdrawPacket.Slot, withdrawPacket.Amount));
            return;
        }

        await session.EmitEventAsync(new AccountWarehouseWithdrawItemEvent(withdrawPacket.Slot, withdrawPacket.Amount));
    }
}