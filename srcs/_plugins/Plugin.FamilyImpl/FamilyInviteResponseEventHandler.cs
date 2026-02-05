using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Communication.Families;
using WingsAPI.Packets.Enums.Chat;
using WingsAPI.Packets.Enums.Families;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.InterChannel;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.FamilyImpl
{
    public class FamilyInviteResponseEventHandler : IAsyncEventProcessor<FamilyInviteResponseEvent>
    {
        private readonly IFamilyInvitationService _familyInvitation;
        private readonly IFamilyManager _familyManager;
        private readonly IGameLanguageService _languageService;
        private readonly ISessionManager _sessionManager;

        public FamilyInviteResponseEventHandler(ISessionManager sessionManager, IGameLanguageService languageService, IFamilyInvitationService familyInvitation, IFamilyManager familyManager)
        {
            _sessionManager = sessionManager;
            _languageService = languageService;
            _familyInvitation = familyInvitation;
            _familyManager = familyManager;
        }

        public async Task HandleAsync(FamilyInviteResponseEvent e, CancellationToken cancellation)
        {
            if (e.Sender.PlayerEntity.IsInFamily())
            {
                return;
            }
            
            IClientSession inviterSession = _sessionManager.GetSessionByCharacterId(e.SenderCharacterId);

            FamilyInvitationGetResponse getInvitation = await _familyInvitation.GetFamilyInvitationAsync(new FamilyInvitationRequest
            {
                SenderId = e.SenderCharacterId,
                TargetId = e.Sender.PlayerEntity.Id
            });

            if (getInvitation.Invitation == null)
            {
                return;
            }

            switch (e.FamilyJoinType)
            {
                case FamilyJoinType.Rejected:
                    await _familyInvitation.RemoveFamilyInvitationAsync(new FamilyInvitationRemoveRequest
                    {
                        SenderId = e.SenderCharacterId
                    });
                    
                    inviterSession?.SendMsgi(MessageType.Default, Game18NConstString.PlayerRefusedFamilyInvite);
                    return;
                
                case FamilyJoinType.PreAccepted:
                    e.Sender.SendDialog($"gjoin {(byte)FamilyJoinType.Accepted} {e.SenderCharacterId}",
                        $"gjoin {(byte)FamilyJoinType.Rejected} {e.SenderCharacterId}", _languageService.GetLanguage(GameDialogKey.FAMILY_DIALOG_ASK_JOIN_CONFIRMATION, e.Sender.UserLanguage));
                    return;
                case FamilyJoinType.Accepted:
                    if (!await _familyManager.CanJoinNewFamilyAsync(e.Sender.PlayerEntity.Id))
                    {
                        await _familyInvitation.RemoveFamilyInvitationAsync(new FamilyInvitationRemoveRequest
                        {
                            SenderId = e.SenderCharacterId
                        });

                        e.Sender.SendMsg(e.Sender.GetLanguage(GameDialogKey.FAMILY_SHOUTMESSAGE_CHANGE_FAMILY_ON_COOLDOWN), MsgMessageType.Middle);
                        return;
                    }

                    await _familyInvitation.RemoveFamilyInvitationAsync(new FamilyInvitationRemoveRequest
                    {
                        SenderId = e.SenderCharacterId
                    });
                    await e.Sender.EmitEventAsync(new FamilyAddMemberEvent(getInvitation.Invitation.SenderFamilyId, getInvitation.Invitation.SenderId));
                    await e.Sender.EmitEventAsync(new FamilyJoinedEvent
                    {
                        FamilyId = getInvitation.Invitation.SenderFamilyId,
                        InviterId = getInvitation.Invitation.SenderId
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}