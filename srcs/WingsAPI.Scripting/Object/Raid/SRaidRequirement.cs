using WingsAPI.Scripting.Attribute;

namespace WingsAPI.Scripting.Object.Raid
{
    [ScriptObject]
    public class SRaidRequirement
    {
        public byte MinimumLevel { get; set; }
        public byte MaximumLevel { get; set; }
        public byte MinimumHeroLevel { get; set; }
        public byte MaximumHeroLevel { get; set; }
        public byte MinimumParticipant { get; set; }
        public byte MaximumParticipant { get; set; }
    }
}