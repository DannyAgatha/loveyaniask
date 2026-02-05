using System.Threading.Tasks;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Game.Relations;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.PacketHandling.Game.Group;

public class RdPacketHandler : GenericGamePacketHandlerBase<RdPacket>
{
    private readonly IInvitationManager _invitationManager;

    public RdPacketHandler(IInvitationManager invitationManager) => _invitationManager = invitationManager;

    protected override async Task HandlePacketAsync(IClientSession session, RdPacket rdPacket)
    {
        if (rdPacket == null)
        {
            return;
        }

        switch (rdPacket.Type)
        {
            case 1:
                switch (rdPacket.Parameter)
                {
                    case null:
                        await session.EmitEventAsync(new RaidPartyInvitePlayerEvent(rdPacket.CharacterId));
                        return;
                    case 1:
                        await session.EmitEventAsync(new RaidPartyJoinEvent(rdPacket.CharacterId, false));
                        return;
                    case 2:
                        if (!_invitationManager.ContainsPendingInvitation(rdPacket.CharacterId, session.PlayerEntity.Id, InvitationType.Raid))
                        {
                            return;
                        }

                        _invitationManager.RemovePendingInvitation(rdPacket.CharacterId, session.PlayerEntity.Id, InvitationType.Raid);
                        return;
                }

                break;

            case 2:

                if (session.PlayerEntity.Raid is { Finished: true })
                {
                    return;
                }

                await session.EmitEventAsync(new RaidPartyLeaveEvent(false));
                break;
            case 3:
                await session.EmitEventAsync(new RaidPartyKickPlayerEvent(rdPacket.CharacterId));
                break;
            case 4:
                await session.EmitEventAsync(new RaidPartyDisbandEvent(true));
                break;
        }
    }
}