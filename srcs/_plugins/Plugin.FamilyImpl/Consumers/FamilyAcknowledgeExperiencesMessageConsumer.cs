using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsAPI.Game.Extensions.Families;
using WingsEmu.Game.Families;
using WingsEmu.Game.Managers;

namespace Plugin.FamilyImpl.Consumers
{
    public class FamilyAcknowledgeExperiencesMessageConsumer : IMessageConsumer<FamilyAcknowledgeExperienceGainedMessage>
    {
        private readonly IFamilyManager _familyManager;
        private readonly ISessionManager _sessionManager;

        public FamilyAcknowledgeExperiencesMessageConsumer(IFamilyManager familyManager, ISessionManager sessionManager)
        {
            _familyManager = familyManager;
            _sessionManager = sessionManager;
        }

        public async Task HandleAsync(FamilyAcknowledgeExperienceGainedMessage e, CancellationToken cancellation)
        {
            IEnumerable<long> familyIds = _familyManager.AddToFamilyExperiences(e.Experiences);

            foreach (long familyId in familyIds)
            {
                FamilyPacketExtensions.SendMembersExpToMembers(_familyManager.GetFamilyByFamilyId(familyId), _sessionManager);
            }
        }
    }
}