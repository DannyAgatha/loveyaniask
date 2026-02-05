// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Linq;
using WingsEmu.Core;

namespace WingsEmu.Game.Families.Configuration;

public class FamilyConfiguration
{
    /*
     * CREATION
     */
    public bool CreationIsGroupRequired { get; set; } = false;
    public int CreationGroupMembersRequired { get; set; } = 3;
    public int CreationPrice { get; set; } = 200_000;
    public int MinimumNameLength { get; set; } = 3;
    public int MaximumNameLength { get; set; } = 20;
    public List<FamilyBuff> FamilyBuffs { get; set; } = new();
    public int DeputyLimit { get; set; } = 2;
    public int KeeperLimit { get; set; } = 999;

    public TimeSpan TimeBetweenFamilyRejoin { get; set; } = TimeSpan.FromDays(1);
    public byte DefaultMembershipCapacity { get; set; } = 20;

    public HashSet<FamilyUpgradesConfiguration> Upgrades { get; set; } = new();

    public List<LevelExperience> Levels { get; set; } = new()
    {
        new()
        {
            Level = 1,
            ExperienceRange = new Range<long>
            {
                Minimum = 0,
                Maximum = 99_999
            }
        },
        new()
        {
            Level = 2,
            ExperienceRange = new Range<long>
            {
                Minimum = 100_000,
                Maximum = 219_999
            }
        },
        new()
        {
            Level = 3,
            ExperienceRange = new Range<long>
            {
                Minimum = 220_000,
                Maximum = 369_999
            }
        }
    };

    public byte GetLevelByFamilyXp(long familyXp)
    {
        LevelExperience levelInfo = Levels.FirstOrDefault(x => x.ExperienceRange.Minimum <= familyXp && familyXp <= x.ExperienceRange.Maximum);
        return levelInfo?.Level ?? default;
    }

    public Range<long> GetRangeByFamilyXp(long familyXp)
    {
        LevelExperience levelInfo = Levels.FirstOrDefault(x => x.ExperienceRange.Minimum <= familyXp && familyXp <= x.ExperienceRange.Maximum);
        return levelInfo?.ExperienceRange;
    }
}