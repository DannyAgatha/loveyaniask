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
    public class FamilyCharacterJoinMessageConsumer : IMessageConsumer<FamilyCharacterJoinMessage>
    {
        private readonly IFamilyManager _family;
        private readonly IGameLanguageService _gameLanguage;
        private readonly ISessionManager _sessionManager;

        public FamilyCharacterJoinMessageConsumer(IFamilyManager family, IGameLanguageService gameLanguage, ISessionManager sessionManager)
        {
            _family = family;
            _gameLanguage = gameLanguage;
            _sessionManager = sessionManager;
        }

        public async Task HandleAsync(FamilyCharacterJoinMessage notification, CancellationToken token)
        {
            long? familyId = notification.FamilyId;

            if (familyId == null)
            {
                return;
            }

            IFamily family = _family.GetFamilyByFamilyId(familyId.Value);
            family?.SendOnlineStatusToMembers(_sessionManager, notification.CharacterId, true, _gameLanguage);
        }
    }
}