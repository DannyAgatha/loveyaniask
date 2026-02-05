// NosEmu
// 


using System.Threading.Tasks;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Families;

public class CreateFamilyPacketHandler : GenericGamePacketHandlerBase<CreateFamilyPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, CreateFamilyPacket packet)
    {
        if (string.IsNullOrEmpty(packet.FamilyName))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(packet.FamilyName))
        {
            return;
        }

        await session.EmitEventAsync(new FamilyCreateEvent
        {
            Name = packet.FamilyName
        });
    }
}