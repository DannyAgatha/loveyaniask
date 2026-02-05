using System.Threading.Tasks;
using WingsAPI.Packets.ClientPackets;
using WingsEmu.Game.Networking;

namespace WingsEmu.Plugins.PacketHandling.Game.BattlePass
{
    public class CloseBattlePassPacketHandler : GenericGamePacketHandlerBase<BpClosePacket>
    {
        protected override async Task HandlePacketAsync(IClientSession session, BpClosePacket packet)
        {
            session.PlayerEntity.HaveBpUiOpen = false;
        }
    }
}