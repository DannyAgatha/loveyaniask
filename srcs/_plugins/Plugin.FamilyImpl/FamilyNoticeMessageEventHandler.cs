using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Families;

namespace Plugin.FamilyImpl
{
    public class FamilyNoticeMessageEventHandler : IAsyncEventProcessor<FamilyNoticeMessageEvent>
    {
        private readonly IGameLanguageService _gameLanguage;
        private readonly IMessagePublisher<FamilyNoticeMessage> _messagePublisher;

        public FamilyNoticeMessageEventHandler(IGameLanguageService gameLanguage, IMessagePublisher<FamilyNoticeMessage> messagePublisher)
        {
            _gameLanguage = gameLanguage;
            _messagePublisher = messagePublisher;
        }

        public async Task HandleAsync(FamilyNoticeMessageEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;

            if (!session.PlayerEntity.IsInFamily())
            {
                session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_NO_FAMILY, session.UserLanguage));
                return;
            }

            IFamily family = session.PlayerEntity.Family;
            FamilyAuthority authorityType = session.PlayerEntity.GetFamilyAuthority();

            if (authorityType == FamilyAuthority.Keeper && !family.AssistantCanNotice || authorityType == FamilyAuthority.Member)
            {
                session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_NO_FAMILY_RIGHT, session.UserLanguage));
                return;
            }

            string message = e.Message;

            if (string.IsNullOrEmpty(message))
            {
                message = string.Empty;
            }

            if (message.Length > 50)
            {
                message = message.Substring(0, 50);
            }

            await _messagePublisher.PublishAsync(new FamilyNoticeMessage
            {
                FamilyId = family.Id,
                Message = e.CleanMessage ? null : message
            });

            await session.EmitEventAsync(new FamilyMessageSentEvent
            {
                Message = message,
                MessageType = FamilyMessageType.Notice
            });
        }
    }
}