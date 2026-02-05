using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Communication.Families;
using WingsAPI.Communication.Player;
using WingsAPI.Packets.Enums.Families;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.InterChannel;
using WingsEmu.Game.Managers;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.FamilyImpl
{
    public class FamilyReceiveInviteEventHandler : IAsyncEventProcessor<FamilyReceiveInviteEvent>
    {
        private readonly IFamilyInvitationService _familyInvitation;
        private readonly IGameLanguageService _languageService;
        private readonly ISessionManager _sessionManager;

        public FamilyReceiveInviteEventHandler(IGameLanguageService languageService, IFamilyInvitationService familyInvitation, ISessionManager sessionManager)
        {
            _languageService = languageService;
            _familyInvitation = familyInvitation;
            _sessionManager = sessionManager;
        }

        public async Task HandleAsync(FamilyReceiveInviteEvent e, CancellationToken cancellation)
        {
            if (e.Sender.PlayerEntity.IsInFamily())
            {
                return;
            }

            if (e.Sender.PlayerEntity.FamilyRequestBlocked)
            {
                await e.Sender.EmitEventAsync(new InterChannelSendInfoByCharIdEvent(e.SenderCharacterId, GameDialogKey.FAMILY_INFO_INVITATION_NOT_ALLOWED));
                return;
            }

            if (e.Sender.PlayerEntity.IsBlocking(e.SenderCharacterId))
            {
                await e.Sender.EmitEventAsync(new InterChannelSendInfoByCharIdEvent(e.SenderCharacterId, GameDialogKey.BLACKLIST_INFO_BLOCKED));
                return;
            }

            await _familyInvitation.SaveFamilyInvitationAsync(new FamilyInvitationSaveRequest
            {
                Invitation = new FamilyInvitation
                {
                    SenderId = e.SenderCharacterId,
                    SenderFamilyId = e.FamilyId,
                    TargetId = e.Sender.PlayerEntity.Id
                }
            });

            e.Sender.SendDlgi2(
                new JoinFamilyPacket { Type = (byte)FamilyJoinType.PreAccepted, CharacterId = e.SenderCharacterId },
                new JoinFamilyPacket { Type = (byte)FamilyJoinType.Rejected, CharacterId = e.SenderCharacterId },
                Game18NConstString.AskMemberFamily,
                1,
                e.FamilyName
            );
        }
    }
}