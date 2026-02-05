using System.Threading.Tasks;
using WingsAPI.Packets.ClientPackets;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Networking;

namespace WingsEmu.Plugins.PacketHandling.Game.Families;

public class FwsPacketHandler : GenericGamePacketHandlerBase<FwsPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, FwsPacket packet)
    {
        await session.EmitEventAsync(new FamilyBuffEvent
        {
            ItemVnum = packet.ItemVnum
        });
    }
}