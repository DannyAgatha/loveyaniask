using System.Collections.Concurrent;
using System.Collections.Generic;
using WingsEmu.Game.Families;

namespace Plugin.FamilyImpl
{
    public class FamilyExperienceManager : IFamilyExperienceManager
    {
        private readonly ConcurrentQueue<ExperienceGainedSubMessage> _bufferFamilyExperience;

        public FamilyExperienceManager() => _bufferFamilyExperience = new ConcurrentQueue<ExperienceGainedSubMessage>();

        public void SaveFamilyExperienceToBuffer(ExperienceGainedSubMessage xpGained)
        {
            _bufferFamilyExperience.Enqueue(xpGained);
        }

        public IReadOnlyCollection<ExperienceGainedSubMessage> GetFamilyExperiencesInBuffer()
        {
            if (_bufferFamilyExperience.IsEmpty)
            {
                return null;
            }

            var list = new List<ExperienceGainedSubMessage>();
            while (_bufferFamilyExperience.TryDequeue(out ExperienceGainedSubMessage exp))
            {
                list.Add(exp);
            }

            return list;
        }
    }
}