using System.Threading.Tasks;
using WingsEmu.Game.Miniland.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Miniland;

public class AddobjPacketHandler : GenericGamePacketHandlerBase<AddobjPacket>
{
    protected override Task HandlePacketAsync(IClientSession session, AddobjPacket packet) =>
        packet == null ? Task.CompletedTask : session.EmitEventAsync(new AddObjMinilandEvent(packet.Slot, packet.PositionX, packet.PositionY));
}