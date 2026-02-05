using System.Threading.Tasks;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Inventory;

public class PutPacketHandler : GenericGamePacketHandlerBase<PutPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, PutPacket putPacket)
    {
        await session.EmitEventAsync(new InventoryDropItemEvent(putPacket.InventoryType, putPacket.Slot, putPacket.Amount));
    }
}