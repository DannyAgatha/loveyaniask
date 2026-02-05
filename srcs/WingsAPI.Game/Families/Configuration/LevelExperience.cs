using WingsEmu.Core;

namespace WingsEmu.Game.Families.Configuration;

public class LevelExperience
{
    public byte Level { get; set; }

    public Range<long> ExperienceRange { get; set; }
}