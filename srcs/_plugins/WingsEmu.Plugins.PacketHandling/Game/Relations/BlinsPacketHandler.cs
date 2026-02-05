using System.Threading.Tasks;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Relations;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class BlInsPacketHandler : GenericGamePacketHandlerBase<BlInsPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, BlInsPacket packet)
    {
        if (session.PlayerEntity.MapInstance.IsAct6PvpInstance)
        {
            return;
        }
        
        await session.EmitEventAsync(new RelationBlockEvent
        {
            CharacterId = packet.CharacterId
        });
    }
}