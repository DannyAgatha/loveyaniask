using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsAPI.Game.Extensions.Families;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Families;
using WingsEmu.Game.Managers;

namespace Plugin.FamilyImpl.Consumers
{
    public class FamilyCharacterLeaveMessageConsumer : IMessageConsumer<FamilyCharacterLeaveMessage>
    {
        private readonly IFamilyManager _familyManager;
        private readonly IGameLanguageService _gameLanguage;
        private readonly ISessionManager _sessionManager;

        public FamilyCharacterLeaveMessageConsumer(IFamilyManager familyManager, IGameLanguageService gameLanguage, ISessionManager sessionManager)
        {
            _familyManager = familyManager;
            _gameLanguage = gameLanguage;
            _sessionManager = sessionManager;
        }

        public async Task HandleAsync(FamilyCharacterLeaveMessage notification, CancellationToken token)
        {
            long characterId = notification.CharacterId;
            long familyId = notification.FamilyId;

            IFamily family = _familyManager.GetFamilyByFamilyId(familyId);
            family?.SendOnlineStatusToMembers(_sessionManager, characterId, false, _gameLanguage);
        }
    }
}