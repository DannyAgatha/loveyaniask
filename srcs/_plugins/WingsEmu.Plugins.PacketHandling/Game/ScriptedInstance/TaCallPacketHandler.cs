using System.Threading.Tasks;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.ScriptedInstance;

public class TaCallPacketHandler : GenericGamePacketHandlerBase<TaCallPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, TaCallPacket packet)
    {
    }
}