using System.Collections.Generic;
using System.Linq;

namespace WingsEmu.Game.Configurations
{
    public class StaticTrainerSpecialistPetSkillsLearningConfiguration
    {
        public static TrainerSpecialistPetSkillsLearningConfiguration Instance { get; private set; }

        public static void Initialize(TrainerSpecialistPetSkillsLearningConfiguration instance)
        {
            Instance = instance;
        }
    }
    
    public class TrainerSpecialistPetSkillsLearningConfiguration
    {
        public List<HeroicLevelConfiguration> Skills { get; set; }
        public List<int> UpgradeSkillsHeroLevel { get; set; }
        
        public int? GetFirstHeroicLevel() => Skills.FirstOrDefault()?.HeroicLevel;

    }

    public class HeroicLevelConfiguration
    {
        public int HeroicLevel { get; set; }
        public List<int> PossibleSkills { get; set; }
    }
}