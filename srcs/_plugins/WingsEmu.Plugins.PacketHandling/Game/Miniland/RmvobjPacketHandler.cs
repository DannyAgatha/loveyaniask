using System.Threading.Tasks;
using WingsEmu.Game.Miniland.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Miniland;

public class RmvobjPacketHandler : GenericGamePacketHandlerBase<RmvobjPacket>
{
    protected override Task HandlePacketAsync(IClientSession session, RmvobjPacket packet) =>
        session.EmitEventAsync(new RmvObjMinilandEvent(packet.Slot));
}