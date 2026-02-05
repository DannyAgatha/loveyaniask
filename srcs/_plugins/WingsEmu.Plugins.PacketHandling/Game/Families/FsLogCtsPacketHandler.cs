using System.Threading.Tasks;
using WingsAPI.Packets.ClientPackets;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Networking;

namespace WingsEmu.Plugins.PacketHandling.Game.Families;

public class FsLogCtsPacketHandler : GenericGamePacketHandlerBase<FsLogCtsPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, FsLogCtsPacket packet)
    {
        await session.EmitEventAsync(new FamilyWarehouseLogsOpenEvent
        {
            Refresh = true
        });
    }
}