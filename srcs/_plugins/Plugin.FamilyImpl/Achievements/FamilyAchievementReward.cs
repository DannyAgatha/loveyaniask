using WingsAPI.Data.Families;

namespace Plugin.FamilyImpl.Achievements
{
    public class FamilyAchievementReward
    {
        public FamilyUpgradeType? FamilyUpgradeCategory { get; set; }
        public short? UpgradeValue { get; set; }
        
        public short? UpgradeSecondValue { get; set; }
        public int? UpgradeId { get; set; }
        public int? FamilyXp { get; set; }
    }
}