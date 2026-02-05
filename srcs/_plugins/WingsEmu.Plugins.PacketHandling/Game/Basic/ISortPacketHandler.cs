using System;
using System.Threading.Tasks;
using WingsAPI.Packets.ClientPackets;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class ISortPacketHandler : GenericGamePacketHandlerBase<ISortPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, ISortPacket packet)
    {
        if (!Enum.TryParse(packet.InventoryType.ToString(), out InventoryType inventoryType))
        {
            return;
        }

        await session.EmitEventAsync(new InventorySortItemEvent
        {
            InventoryType = inventoryType,
            Confirm = packet.Confirm
        });
    }
}