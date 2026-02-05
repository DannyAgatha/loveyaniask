using System.Collections.Generic;

namespace Plugin.FamilyImpl.Achievements
{
    public class FamilyMissionSpecificConfiguration
    {
        public int MissionId { get; set; }
        public int Value { get; set; }
        public int MinimumRequiredLevel { get; set; }
        public bool OncePerPlayerPerDay { get; set; } = false;
        public List<FamilyMissionReward> Rewards { get; set; }
    }
}