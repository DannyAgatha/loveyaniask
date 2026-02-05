using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Achievements;
using Plugin.FamilyImpl.Messages;
using WingsAPI.Communication.Families;
using WingsAPI.Game.Extensions.Families;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Families;

namespace Plugin.FamilyImpl
{
    public class FamilyTodayEventHandler : IAsyncEventProcessor<FamilyTodayEvent>
    {
        private readonly IFamilyAchievementManager _familyAchievement;
        private readonly IFamilyService _familyService;
        private readonly IGameLanguageService _gameLanguage;
        private readonly IMessagePublisher<FamilyMemberTodayMessage> _messagePublisher;

        public FamilyTodayEventHandler(IGameLanguageService gameLanguage, IMessagePublisher<FamilyMemberTodayMessage> messagePublisher, IFamilyService familyService,
            IFamilyAchievementManager familyAchievement)
        {
            _gameLanguage = gameLanguage;
            _messagePublisher = messagePublisher;
            _familyService = familyService;
            _familyAchievement = familyAchievement;
        }

        public async Task HandleAsync(FamilyTodayEvent e, CancellationToken cancellation)
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

            if (session.PlayerEntity.Level < 30)
            {
                session.SendInfo(session.GetLanguage(GameDialogKey.FAMILY_INFO_TODAY_LOW_LEVEL));
                return;
            }

            FamilyMembership membership = session.PlayerEntity.GetMembershipById(session.PlayerEntity.Id);
            if (membership == null)
            {
                return;
            }

            MembershipTodayResponse response = await _familyService.CanPerformTodayMessageAsync(new MembershipTodayRequest
            {
                CharacterId = session.PlayerEntity.Id,
                CharacterName = session.PlayerEntity.Name
            });

            if (!response.CanPerformAction)
            {
                session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_USED_DAILY_MESSAGE, session.UserLanguage));
                return;
            }


            await session.FamilyAddLogAsync(FamilyLogType.DailyMessage, session.PlayerEntity.Name, message);
            await _messagePublisher.PublishAsync(new FamilyMemberTodayMessage
            {
                CharacterId = session.PlayerEntity.Id,
                Message = message
            });

            // achievements part
            _familyAchievement.IncrementFamilyAchievement(membership.FamilyId, (int)FamilyAchievementsVnum.ENTER_20_QUOTES_OF_THE_DAY);

            await session.EmitEventAsync(new FamilyMessageSentEvent
            {
                Message = message,
                MessageType = FamilyMessageType.Quote
            });
        }
    }
}