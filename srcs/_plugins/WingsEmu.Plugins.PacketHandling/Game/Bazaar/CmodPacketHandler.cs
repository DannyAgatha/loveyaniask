using System.Threading.Tasks;
using WingsAPI.Game.Extensions.Bazaar;
using WingsEmu.Game.Bazaar.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Bazaar;

public class CmodPacketHandler : GenericGamePacketHandlerBase<CmodPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, CmodPacket packet)
    {
        await session.EmitEventAsync(new BazaarItemChangePriceEvent(packet.BazaarId, packet.NewPricePerItem, packet.Confirmed != 0));
    }
}