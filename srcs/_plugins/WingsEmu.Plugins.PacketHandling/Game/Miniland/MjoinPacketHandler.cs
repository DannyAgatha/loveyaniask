using System.Threading.Tasks;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Miniland;

public class MjoinPacketHandler : GenericGamePacketHandlerBase<MJoinPacket>
{
    private readonly IMinilandManager _miniland;
    private readonly ISessionManager _sessionManager;

    public MjoinPacketHandler(ISessionManager sessionManager, IMinilandManager miniland)
    {
        _sessionManager = sessionManager;
        _miniland = miniland;
    }

    protected override async Task HandlePacketAsync(IClientSession session, MJoinPacket packet)
    {
        switch (packet.Type)
        {
            case 0:
                IClientSession target = _sessionManager.GetSessionByCharacterId(packet.CharacterId);
                if (target == null)
                {
                    return;
                }

                if (!_miniland.ContainsMinilandInvite(target.PlayerEntity.Id))
                {
                    return;
                }

                if (!_miniland.ContainsTargetInvite(target.PlayerEntity.Id, session.PlayerEntity.Id))
                {
                    return;
                }

                if (packet.OptionType != 1)
                {
                    _miniland.RemoveMinilandInvite(target.PlayerEntity.Id, session.PlayerEntity.Id);
                    return;
                }

                await target.EmitEventAsync(new InviteJoinMinilandEvent(session.CharacterName(), false));
                break;
            case 1:
                if (packet.OptionType == 1)
                {
                    await session.EmitEventAsync(new MinilandSignPostJoinEvent
                    {
                        PlayerId = packet.CharacterId
                    });
                    return;
                }

                target = _sessionManager.GetSessionByCharacterId(packet.CharacterId);
                if (target == null)
                {
                    return;
                }

                await session.EmitEventAsync(new InviteJoinMinilandEvent(target.CharacterName(), false, true));
                break;
        }
    }
}