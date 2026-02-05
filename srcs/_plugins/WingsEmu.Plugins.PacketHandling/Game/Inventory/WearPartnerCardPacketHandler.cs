using System.Threading.Tasks;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.PacketHandling.Game.Inventory;

public class WearPartnerCardPacketHandler : GenericGamePacketHandlerBase<WearPartnerCardPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, WearPartnerCardPacket packet)
    {
        byte petSlot = packet.PetId;
        byte itemSlot = packet.InventorySlot;

        if (petSlot == 0)
        {
            return;
        }

        await session.EmitEventAsync(new PartnerInventoryEquipItemEvent(petSlot, itemSlot, InventoryType.Specialist));
    }
}