// NosEmu
// 


using System.Threading.Tasks;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Families;

public class FmgPacketHandler : GenericGamePacketHandlerBase<FamilyManagementPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, FamilyManagementPacket packet)
    {
        await session.EmitEventAsync(new FamilyChangeAuthorityEvent(packet.FamilyAuthorityType, packet.TargetId, packet.Confirmed.GetValueOrDefault()));
    }
}