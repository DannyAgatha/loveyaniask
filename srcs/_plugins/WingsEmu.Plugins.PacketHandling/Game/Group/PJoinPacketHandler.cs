using System.Threading.Tasks;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Groups.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Group;

public class PjoinPacketHandler : GenericGamePacketHandlerBase<PJoinPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, PJoinPacket packet)
    {
        if (session.IsActionForbidden())
        {
            return;
        }

        await session.EmitEventAsync(new GroupActionEvent
        {
            CharacterId = packet.CharacterId,
            RequestType = packet.RequestType
        });
    }
}