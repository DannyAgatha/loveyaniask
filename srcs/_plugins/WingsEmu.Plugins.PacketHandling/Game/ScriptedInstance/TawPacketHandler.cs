using System.Threading.Tasks;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.ScriptedInstance;

public class TawPacketHandler : GenericGamePacketHandlerBase<TawPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, TawPacket packet)
    {
    }
}