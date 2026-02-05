using System.Threading.Tasks;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Useless;

public class PdtClosePacketHandler : GenericGamePacketHandlerBase<PdtClosePacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, PdtClosePacket packet)
    {
        session.PlayerEntity.IsCraftingItem = false;
        session.PlayerEntity.HasNosBazaarOpen = false;
        session.PlayerEntity.LastMinilandProducedItem = null;
    }
}