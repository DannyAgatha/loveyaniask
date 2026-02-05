using System.Threading.Tasks;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class GameStartPacketHandler : GenericCharScreenPacketHandlerBase<GameStartPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, GameStartPacket packet)
    {
        await session.EmitEventAsync(new CharacterLoadEvent());
    }
}