using System.Threading.Tasks;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Useless;

public class SnapPacketHandler : GenericGamePacketHandlerBase<SnapPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, SnapPacket packet)
    {
        //we can log when people are taking screenshots ingame so that we can determine if they are legit (at least in some scale) or fake
    }
}