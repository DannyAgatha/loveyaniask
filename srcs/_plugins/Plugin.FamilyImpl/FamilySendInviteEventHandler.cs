using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Families;

namespace Plugin.FamilyImpl
{
    public class FamilySendInviteEventHandler : IAsyncEventProcessor<FamilySendInviteEvent>
    {
        private readonly IGameLanguageService _languageService;
        private readonly IMessagePublisher<FamilyInviteMessage> _messagePublisher;
        private readonly ISessionManager _sessionManager;

        public FamilySendInviteEventHandler(ISessionManager sessionManager, IGameLanguageService languageService, IMessagePublisher<FamilyInviteMessage> messagePublisher)
        {
            _sessionManager = sessionManager;
            _languageService = languageService;
            _messagePublisher = messagePublisher;
        }

        public async Task HandleAsync(FamilySendInviteEvent e, CancellationToken cancellation)
        {
            IFamily family = e.Sender.PlayerEntity.Family;

            if (family == null)
            {
                e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.FAMILY_INFO_NOT_IN_FAMILY, e.Sender.UserLanguage));
                return;
            }

            if (!e.Sender.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
            {
                e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_MUST_BE_IN_CLASSIC_MAP, e.Sender.UserLanguage));
                return;
            }

            if (e.Sender.CantPerformActionOnAct4())
            {
                return;
            }

            FamilyAuthority sessionAuthority = e.Sender.PlayerEntity.GetFamilyAuthority();
            if (sessionAuthority == FamilyAuthority.Member)
            {
                e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.FAMILY_INFO_NO_FAMILY_RIGHT, e.Sender.UserLanguage));
                return;
            }

            if (sessionAuthority == FamilyAuthority.Keeper && !family.AssistantCanInvite)
            {
                e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.FAMILY_INFO_NO_FAMILY_RIGHT, e.Sender.UserLanguage));
                return;
            }

            if (family.Members.Count >= family.GetMaximumMembershipCapacity())
            {
                e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.FAMILY_INFO_FULL, e.Sender.UserLanguage));
                return;
            }

            IClientSession localReceiverSession = _sessionManager.GetSessionByCharacterName(e.ReceiverNickname);
            if (localReceiverSession == null)
            {
                if (!_sessionManager.IsOnline(e.ReceiverNickname))
                {
                    e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.INFORMATION_INFO_PLAYER_OFFLINE, e.Sender.UserLanguage));
                    return;
                }

                await _messagePublisher.PublishAsync(new FamilyInviteMessage
                {
                    FamilyId = family.Id,
                    FamilyName = family.Name,
                    ReceiverNickname = e.ReceiverNickname,
                    SenderCharacterId = e.Sender.PlayerEntity.Id
                }, cancellation);
                return;
            }

            if (localReceiverSession.PlayerEntity.RainbowBattleComponent.IsInRainbowBattle)
            {
                return;
            }
            
            if (localReceiverSession.PlayerEntity.MapInstance.IsAct6PvpInstance)
            {
                return;
            }

            await e.Sender.EmitEventAsync(new FamilyInvitedEvent { TargetId = localReceiverSession.PlayerEntity.Id });
            await localReceiverSession.EmitEventAsync(new FamilyReceiveInviteEvent(family.Name, e.Sender.PlayerEntity.Id, family.Id));
        }
    }
}