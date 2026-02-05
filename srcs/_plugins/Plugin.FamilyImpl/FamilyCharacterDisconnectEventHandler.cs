using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Families;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Families;
using WingsEmu.Game.Managers;

namespace Plugin.FamilyImpl
{
    public class FamilyCharacterDisconnectEventHandler : IAsyncEventProcessor<CharacterDisconnectedEvent>
    {
        private readonly IFamilyManager _familyManager;
        private readonly IGameLanguageService _gameLanguage;
        private readonly ISessionManager _sessionManager;

        public FamilyCharacterDisconnectEventHandler(IFamilyManager familyManager, ISessionManager sessionManager, IGameLanguageService gameLanguage)
        {
            _familyManager = familyManager;
            _sessionManager = sessionManager;
            _gameLanguage = gameLanguage;
        }

        public async Task HandleAsync(CharacterDisconnectedEvent e, CancellationToken cancellation)
        {
            IFamily family = e.Sender.PlayerEntity.Family;
            if (family == null)
            {
                return;
            }

            _familyManager.MemberDisconnectionUpdate(e.Sender.PlayerEntity.Id, e.DisconnectionTime);
            family.SendOnlineStatusToMembers(_sessionManager, e.Sender.PlayerEntity.Id, false, _gameLanguage);
        }
    }
}