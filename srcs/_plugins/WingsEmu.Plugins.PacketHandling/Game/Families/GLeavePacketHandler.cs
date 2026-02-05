using System.Threading.Tasks;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Families;

public class GLeavePacketHandler : GenericGamePacketHandlerBase<GLeavePacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, GLeavePacket packet)
    {
        await session.EmitEventAsync(new FamilyLeaveEvent());
    }
}