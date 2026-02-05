using System.Collections.Generic;
using WingsAPI.Scripting.Attribute;

namespace WingsAPI.Scripting.Object.Timespace
{
    [ScriptObject]
    public class STimeSpaceRewardsObject
    {
        public int? FixedReputation { get; set; }
        public int? ReputationLevelMultiplier { get; set; }
        public bool DefaultReputation { get; set; }

        public IEnumerable<STimespaceItemReward> DrawRewards { get; set; }
        public IEnumerable<STimespaceItemReward> SpecialRewards { get; set; }
        public IEnumerable<STimespaceItemReward> BonusRewards { get; set; }
    }
}