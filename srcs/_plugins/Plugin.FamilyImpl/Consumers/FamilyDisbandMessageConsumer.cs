using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsAPI.Game.Extensions.Families;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Families;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;

namespace Plugin.FamilyImpl.Consumers
{
    public class FamilyDisbandMessageConsumer : IMessageConsumer<FamilyDisbandMessage>
    {
        private readonly IFamilyManager _familyManager;
        private readonly IGameLanguageService _gameLanguageService;
        private readonly ISessionManager _sessionManager;

        public FamilyDisbandMessageConsumer(IFamilyManager familyManager, ISessionManager sessionManager, IGameLanguageService gameLanguageService)
        {
            _familyManager = familyManager;
            _sessionManager = sessionManager;
            _gameLanguageService = gameLanguageService;
        }

        public async Task HandleAsync(FamilyDisbandMessage notification, CancellationToken token)
        {
            long familyId = notification.FamilyId;

            IFamily family = _familyManager.GetFamilyByFamilyId(familyId);
            if (family == null)
            {
                return;
            }

            _familyManager.RemoveFamily(family.Id);
            foreach (FamilyMembership member in family.Members)
            {
                IClientSession memberSession = _sessionManager.GetSessionByCharacterId(member.CharacterId);
                memberSession?.SendResetFamilyInterface();
                memberSession?.BroadcastGidx(null, _gameLanguageService);
            }
        }
    }
}