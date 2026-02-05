using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums.Families;

namespace Plugin.FamilyImpl
{
    public class FamilyShoutEventHandler : IAsyncEventProcessor<FamilyShoutEvent>
    {
        private readonly IGameLanguageService _gameLanguage;
        private readonly IMessagePublisher<FamilyShoutMessage> _messagePublisher;
        private readonly ISessionManager _sessionManager;

        public FamilyShoutEventHandler(IGameLanguageService gameLanguage, IMessagePublisher<FamilyShoutMessage> messagePublisher, ISessionManager sessionManager)
        {
            _gameLanguage = gameLanguage;
            _messagePublisher = messagePublisher;
            _sessionManager = sessionManager;
        }

        public async Task HandleAsync(FamilyShoutEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            string message = e.Message;

            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            if (message.Length > 50)
            {
                message = message.Substring(0, 50);
            }

            if (!session.PlayerEntity.IsInFamily())
            {
                session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_NO_FAMILY, session.UserLanguage));
                return;
            }

            FamilyAuthority senderAuthority = session.PlayerEntity.GetFamilyAuthority();
            if (senderAuthority == FamilyAuthority.Member || senderAuthority == FamilyAuthority.Keeper && !session.PlayerEntity.Family.AssistantCanShout)
            {
                session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_NO_FAMILY_RIGHT, session.UserLanguage));
                return;
            }

            await _sessionManager.BroadcastAsync(async s =>
            {
                string msg = _gameLanguage.GetLanguageFormat(GameDialogKey.FAMILY_SHOUTMESSAGE_SHOUT, s.UserLanguage, session.PlayerEntity.Name, message);
                return session.GenerateMsgPacket(msg, MsgMessageType.Middle);
            }, new FamilyBroadcast(session.PlayerEntity.Family.Id));

            await _messagePublisher.PublishAsync(new FamilyShoutMessage
            {
                Message = message,
                SenderName = session.PlayerEntity.Name,
                FamilyId = session.PlayerEntity.Family.Id,
                GameDialogKey = GameDialogKey.FAMILY_SHOUTMESSAGE_SHOUT
            });

            await session.EmitEventAsync(new FamilyMessageSentEvent
            {
                Message = message,
                MessageType = FamilyMessageType.Shout
            });
        }
    }
}