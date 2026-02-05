using System.Threading.Tasks;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class FlPacketHandler : GenericGamePacketHandlerBase<FlPacket>
{
    private readonly FInsPacketHandler _fInsPacketHandler;
    private readonly ISessionManager _sessionManager;

    public FlPacketHandler(FInsPacketHandler fInsPacketHandler, ISessionManager sessionManager)
    {
        _sessionManager = sessionManager;
        _fInsPacketHandler = fInsPacketHandler;
    }

    protected override async Task HandlePacketAsync(IClientSession session, FlPacket packet)
    {
        IClientSession otherSession = _sessionManager.GetSessionByCharacterName(packet.CharName);

        if (otherSession == null)
        {
            return;
        }

        if (otherSession == session)
        {
            return;
        }

        await _fInsPacketHandler.HandleAsync(session, new FInsPacket { Type = 0, CharacterId = otherSession.PlayerEntity.Id });
    }
}