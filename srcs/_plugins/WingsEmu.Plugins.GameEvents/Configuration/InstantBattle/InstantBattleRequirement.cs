using WingsEmu.Core;

namespace WingsEmu.Plugins.GameEvents.Configuration.InstantBattle
{
    public class InstantBattleRequirement
    {
        public Range<short> Players { get; set; }
        public Range<byte> Level { get; set; }
        public Range<byte> HeroicLevel { get; set; }
    }
}