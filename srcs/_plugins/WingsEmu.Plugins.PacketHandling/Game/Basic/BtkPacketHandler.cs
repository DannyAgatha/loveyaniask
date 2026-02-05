using System.Threading.Tasks;
using WingsEmu.Game.Chat;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class BtkPacketHandler : GenericGamePacketHandlerBase<BtkPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, BtkPacket btkPacket)
    {
        await session.EmitEventAsync(new ChatSendFriendMessageEvent
        {
            Message = btkPacket.Message,
            TargetId = btkPacket.CharacterId
        });
    }
}