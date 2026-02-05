using System.Threading.Tasks;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class RStartPacketHandler : GenericGamePacketHandlerBase<RStartPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, RStartPacket packet)
    {
        if (packet.Type != 1)
        {
            return;
        }

        await session.EmitEventAsync(new TimeSpaceStartPortalEvent());
    }
}