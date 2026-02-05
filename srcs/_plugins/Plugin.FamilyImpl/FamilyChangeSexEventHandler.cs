using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.FamilyImpl
{
    public class FamilyChangeSexEventHandler : IAsyncEventProcessor<FamilyChangeSexEvent>
    {
        private readonly IGameLanguageService _gameLanguage;
        private readonly IMessagePublisher<FamilyHeadSexMessage> _messagePublisher;
        private readonly ISessionManager _sessionManager;

        public FamilyChangeSexEventHandler(IGameLanguageService gameLanguage, IMessagePublisher<FamilyHeadSexMessage> messagePublisher, ISessionManager sessionManager)
        {
            _gameLanguage = gameLanguage;
            _messagePublisher = messagePublisher;
            _sessionManager = sessionManager;
        }

        public async Task HandleAsync(FamilyChangeSexEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;

            if (!Enum.TryParse(e.Gender.ToString(), out GenderType genderType))
            {
                return;
            }

            if (!session.PlayerEntity.IsInFamily())
            {
                session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_NO_FAMILY, session.UserLanguage));
                return;
            }

            if (!session.PlayerEntity.IsHeadOfFamily())
            {
                session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_NO_FAMILY_RIGHT, session.UserLanguage));
                return;
            }

            if (session.PlayerEntity.Family.HeadGender == genderType)
            {
                return;
            }

            _sessionManager.Broadcast(x => { return session.GenerateMsgPacket(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_SHOUTMESSAGE_HEAD_CHANGE_SEX, x.UserLanguage), MsgMessageType.Middle); });

            await _messagePublisher.PublishAsync(new FamilyHeadSexMessage
            {
                FamilyId = session.PlayerEntity.Family.Id,
                NewGenderType = genderType
            });
        }
    }
}