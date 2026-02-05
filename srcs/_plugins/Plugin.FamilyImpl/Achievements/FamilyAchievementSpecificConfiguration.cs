using System.Collections.Generic;

namespace Plugin.FamilyImpl.Achievements
{
    public class FamilyAchievementSpecificConfiguration
    {
        public int Id { get; set; }
        public int Value { get; set; }
        public int? RequiredId { get; set; }
        public List<FamilyAchievementReward> Rewards { get; set; }
        public int? RequiredLevel { get; set; }

    }
}