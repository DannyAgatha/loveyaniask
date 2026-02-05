using System.Threading.Tasks;
using WingsEmu.Game.Miniland.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Miniland;

public class UseobjPacketHandler : GenericGamePacketHandlerBase<UseobjPacket>
{
    protected override Task HandlePacketAsync(IClientSession session, UseobjPacket packet)
        => packet == null ? Task.CompletedTask : session.EmitEventAsync(new UseObjMinilandEvent(packet.CharacterName, packet.Slot));
}