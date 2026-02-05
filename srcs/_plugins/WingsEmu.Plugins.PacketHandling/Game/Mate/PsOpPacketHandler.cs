using System.Threading.Tasks;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Mate;

public class PsOpPacketHandler : GenericGamePacketHandlerBase<PsopServerPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, PsopServerPacket packet)
    {
        await session.EmitEventAsync(new PartnerSpecialistSkillEvent
        {
            PartnerSlot = packet.PetSlot,
            SkillSlot = packet.SkillSlot,
            Roll = packet.Option == 1
        });
    }
}