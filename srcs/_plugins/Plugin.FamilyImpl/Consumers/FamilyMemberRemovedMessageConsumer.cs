using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsAPI.Game.Extensions.Families;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Configuration;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;

namespace Plugin.FamilyImpl.Consumers
{
    public class FamilyMemberRemovedMessageConsumer : IMessageConsumer<FamilyMemberRemovedMessage>
    {
        private readonly FamilyConfiguration _familyConfiguration;
        private readonly IFamilyManager _familyManager;
        private readonly IGameLanguageService _languageService;
        private readonly ISessionManager _sessionManager;

        public FamilyMemberRemovedMessageConsumer(IFamilyManager familyManager, ISessionManager sessionManager, IGameLanguageService languageService, FamilyConfiguration familyConfiguration)
        {
            _familyManager = familyManager;
            _sessionManager = sessionManager;
            _languageService = languageService;
            _familyConfiguration = familyConfiguration;
        }

        public async Task HandleAsync(FamilyMemberRemovedMessage e, CancellationToken cancellation)
        {
            _familyManager.RemoveMember(e.CharacterId, e.FamilyId);

            IClientSession localSession = _sessionManager.GetSessionByCharacterId(e.CharacterId);
            localSession?.SendResetFamilyInterface();
            localSession?.BroadcastGidx(null, _languageService);

            Family family = _familyManager.GetFamilyByFamilyId(e.FamilyId);
            FamilyPacketExtensions.SendFamilyMembersInfoToMembers(family, _sessionManager, _familyConfiguration);
        }
    }
}