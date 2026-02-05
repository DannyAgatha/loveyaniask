using System.Collections.Generic;
using WingsEmu.Game.Families;

namespace Plugin.FamilyImpl
{
    public interface IFamilyExperienceManager
    {
        void SaveFamilyExperienceToBuffer(ExperienceGainedSubMessage xpGained);
        IReadOnlyCollection<ExperienceGainedSubMessage> GetFamilyExperiencesInBuffer();
    }
}