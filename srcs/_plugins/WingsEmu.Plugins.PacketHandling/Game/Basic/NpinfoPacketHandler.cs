using System.Threading.Tasks;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class NpinfoPacketHandler : GenericGamePacketHandlerBase<NpinfoPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, NpinfoPacket npInfoPacket)
    {
        session.SendPClearPacket();
        session.SendScnPackets();
        session.SendScpPackets(npInfoPacket.Page);
    }
}