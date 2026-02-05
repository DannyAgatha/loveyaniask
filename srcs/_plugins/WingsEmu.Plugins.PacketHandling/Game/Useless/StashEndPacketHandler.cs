using System.Threading.Tasks;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Useless;

public class StashEndPacketHandler : GenericGamePacketHandlerBase<StashEndPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, StashEndPacket packet)
    {
        session.PlayerEntity.IsWarehouseOpen = false;
        session.PlayerEntity.HasNosBazaarOpen = false;
        session.PlayerEntity.IsPartnerWarehouseOpen = false;
    }
}