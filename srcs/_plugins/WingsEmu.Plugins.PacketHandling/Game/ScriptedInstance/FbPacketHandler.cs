using System.Threading.Tasks;
using WingsAPI.Packets.ClientPackets;
using WingsEmu.Game.Networking;
using WingsEmu.Game.RainbowBattle.Event;

namespace WingsEmu.Plugins.PacketHandling.Game.ScriptedInstance;

public class FbPacketHandler : GenericGamePacketHandlerBase<FbPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, FbPacket packet)
    {
        if (!session.PlayerEntity.RainbowBattleComponent.IsInRainbowBattle)
        {
            return;
        }

        await session.EmitEventAsync(new RainbowBattleLeaveEvent
        {
            SendMessage = true
        });
    }
}