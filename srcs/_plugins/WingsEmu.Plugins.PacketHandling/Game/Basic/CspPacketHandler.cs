using System.Threading.Tasks;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class CspPacketHandler : GenericGamePacketHandlerBase<CspServerPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, CspServerPacket packet)
    {
        if (session.PlayerEntity.Id != packet.CharacterId)
        {
            return;
        }

        if (!session.PlayerEntity.IsUsingBubble())
        {
            return;
        }

        if (session.PlayerEntity.GetMessage() != packet.Message)
        {
            session.PlayerEntity.RemoveBubble();
            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.NORMAL, "Tried to change bubble message");
            return;
        }

        session.BroadcastBubbleMessage(packet.Message);
    }
}