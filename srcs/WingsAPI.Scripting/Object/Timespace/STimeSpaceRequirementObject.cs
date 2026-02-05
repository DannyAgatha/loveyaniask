using WingsAPI.Scripting.Attribute;

namespace WingsAPI.Scripting.Object.Timespace
{
    [ScriptObject]
    public class STimeSpaceRequirementObject
    {
        public int MinimumLevel { get; set; }
        public int MaximumLevel { get; set; }
        public int MinimumHeroLevel { get; set; }
        public int MaximumHeroLevel { get; set; }
        public int MinimumParticipant { get; set; }
        public int MaximumParticipant { get; set; }
        public short SeedOfPowerCost { get; set; }
    }
}