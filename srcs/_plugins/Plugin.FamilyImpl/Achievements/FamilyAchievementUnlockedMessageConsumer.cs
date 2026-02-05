using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.FamilyImpl.Achievements
{
    public class FamilyAchievementUnlockedMessageConsumer : IMessageConsumer<FamilyAchievementUnlockedMessage>
    {
        private readonly IFamilyManager _familyManager;
        private readonly IGameLanguageService _gameLanguage;
        private readonly IItemsManager _itemsManager;
        private readonly ISessionManager _sessionManager;

        public FamilyAchievementUnlockedMessageConsumer(ISessionManager sessionManager, IFamilyManager familyManager, IItemsManager itemsManager, IGameLanguageService gameLanguage)
        {
            _sessionManager = sessionManager;
            _familyManager = familyManager;
            _itemsManager = itemsManager;
            _gameLanguage = gameLanguage;
        }

        public async Task HandleAsync(FamilyAchievementUnlockedMessage notification, CancellationToken token)
        {
            Family family = _familyManager.GetFamilyByFamilyIdCache(notification.FamilyId);
            if (family == null)
            {
                return;
            }

            IGameItem quest = _itemsManager.GetItem(notification.AchievementId + 600);
            if (quest == null)
            {
                return;
            }

            foreach (FamilyMembership familyMember in family.Members)
            {
                IClientSession session = _sessionManager.GetSessionByCharacterId(familyMember.CharacterId);
                if (session == null)
                {
                    continue;
                }

                string language = _gameLanguage.GetLanguage(GameDataType.Item, quest.Name, session.UserLanguage);
                session.SendMsg(session.GetLanguageFormat(GameDialogKey.FAMILY_ACHIEVEMENT_UNLOCKED, language), MsgMessageType.Middle);
                session.SendInformationChatMessage(session.GetLanguageFormat(GameDialogKey.FAMILY_ACHIEVEMENT_UNLOCKED, language));
            }
        }
    }
}