// NosEmu
// 


using System.Threading.Tasks;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Families;

public class FamilyDisbandPacketHandler : GenericGamePacketHandlerBase<FamilyDisbandPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, FamilyDisbandPacket packet)
    {
        await session.EmitEventAsync(new FamilyDisbandEvent());
    }
}