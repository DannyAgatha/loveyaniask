using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FamilyServer.Managers;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsEmu.Game.Families;

namespace FamilyServer.Consumers
{
    public class FamilyDeclareExperienceGainedMessageConsumer : IMessageConsumer<FamilyDeclareExperienceGainedMessage>
    {
        private readonly FamilyExperienceManager _familyExperienceManager;

        public FamilyDeclareExperienceGainedMessageConsumer(FamilyExperienceManager familyExperienceManager) => _familyExperienceManager = familyExperienceManager;

        public async Task HandleAsync(FamilyDeclareExperienceGainedMessage e, CancellationToken cancellation)
        {
            Dictionary<long, long> dictionary = FuseDifferentXpValues(e.Experiences);

            foreach ((long characterId, long experienceToAdd) in dictionary)
            {
                _familyExperienceManager.AddExperienceIncrementRequest(new ExperienceIncrementRequest
                {
                    CharacterId = characterId,
                    Experience = experienceToAdd
                });
            }
        }

        private static Dictionary<long, long> FuseDifferentXpValues(IEnumerable<ExperienceGainedSubMessage> experienceGainedSubMessages)
        {
            var dictionary = new Dictionary<long, long>();
            foreach (ExperienceGainedSubMessage exp in experienceGainedSubMessages)
            {
                if (dictionary.ContainsKey(exp.CharacterId))
                {
                    dictionary[exp.CharacterId] += exp.FamXpGained;
                    continue;
                }

                dictionary.Add(exp.CharacterId, exp.FamXpGained);
            }

            return dictionary;
        }
    }
}