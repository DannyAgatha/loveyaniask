using System.Threading.Tasks;
using WingsEmu.Game.Groups.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Group;

public class PLeavePacketHandler : GenericGamePacketHandlerBase<PLeavePacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, PLeavePacket packet)
    {
        await session.EmitEventAsync(new LeaveGroupEvent());
    }
}