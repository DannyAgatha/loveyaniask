using System.Threading.Tasks;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Useless;

public class LbsPacketHandler : GenericGamePacketHandlerBase<LbsPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, LbsPacket packet)
    {
    }
}