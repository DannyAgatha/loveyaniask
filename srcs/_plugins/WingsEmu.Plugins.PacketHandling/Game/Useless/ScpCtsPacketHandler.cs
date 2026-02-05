using System.Threading.Tasks;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Useless;

public class ScpCtsPacketHandler : GenericGamePacketHandlerBase<ScpCtsPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, ScpCtsPacket packet)
    {
    }
}