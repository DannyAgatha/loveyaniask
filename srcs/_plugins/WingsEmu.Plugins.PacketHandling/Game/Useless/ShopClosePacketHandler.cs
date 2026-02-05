using System.Threading.Tasks;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Useless;

public class ShopClosePacketHandler : GenericGamePacketHandlerBase<ShopClosePacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, ShopClosePacket packet)
    {
    }
}