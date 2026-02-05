using System.Threading.Tasks;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class DirectionPacketHandler : GenericGamePacketHandlerBase<DirectionPacket>
{
    private readonly ISessionManager _sessionManager;

    public DirectionPacketHandler(ISessionManager sessionManager) => _sessionManager = sessionManager;

    protected override async Task HandlePacketAsync(IClientSession session, DirectionPacket packet)
    {
        if (packet.Direction >= 8)
        {
            _sessionManager.BroadcastToGameMaster(session, "DirectionPacketHandler dir > 8");
            return;
        }

        if (session.PlayerEntity.CheatComponent.IsInvisible)
        {
            return;
        }

        switch (packet.VisualType)
        {
            case VisualType.Player:
                if (packet.Id != session.PlayerEntity.Id)
                {
                    _sessionManager.BroadcastToGameMaster(session, "DirectionPacketHandler id != charId");
                    return;
                }

                session.PlayerEntity.Direction = packet.Direction;
                session.CurrentMapInstance?.Broadcast(session.PlayerEntity.GenerateDir());
                break;
            case VisualType.Npc:
                IMateEntity mate = session.PlayerEntity.MateComponent.GetMate(m => m.Id == packet.Id);

                if (mate == null)
                {
                    return;
                }

                mate.Direction = packet.Direction;
                session.CurrentMapInstance?.Broadcast(mate.GenerateDir());
                return;
            case VisualType.Monster:
                _sessionManager.BroadcastToGameMaster(session, "DirectionPacketHandler VisualType.Monster");
                break;
            case VisualType.Object:
                _sessionManager.BroadcastToGameMaster(session, "DirectionPacketHandler VisualType.Object");
                break;
            default:
                _sessionManager.BroadcastToGameMaster(session, "DirectionPacketHandler default");
                break;
        }
    }
}