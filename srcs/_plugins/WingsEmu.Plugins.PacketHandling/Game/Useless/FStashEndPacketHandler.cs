using System.Threading.Tasks;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Useless;

public class FStashEndPacketHandler : GenericGamePacketHandlerBase<FStashEndPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, FStashEndPacket packet)
    {
        session.PlayerEntity.HasNosBazaarOpen = false;
        await session.EmitEventAsync(new FamilyWarehouseCloseEvent());
    }
}