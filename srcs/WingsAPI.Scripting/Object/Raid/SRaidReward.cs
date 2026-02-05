using WingsAPI.Scripting.Attribute;

namespace WingsAPI.Scripting.Object.Raid
{
    [ScriptObject]
    public class SRaidReward
    {
        public SRaidBox RaidBox { get; set; }
        public bool DefaultReputation { get; set; }
        public int? FixedReputation { get; set; }
    }
}