using System.Linq;
using WingsEmu.Game;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Items;
using WingsEmu.Game.Mates.PartnerFusion;

namespace NosEmu.Plugins.BasicImplementations.Algorithms;

public class PartnerFusionAlgorithm : IPartnerFusionAlgorithm
{
    private readonly IRandomGenerator _randomGenerator;
    private readonly PartnerFusionExperienceConfiguration _fusionExperience;

    public PartnerFusionAlgorithm(IRandomGenerator randomGenerator, PartnerFusionExperienceConfiguration fusionExperience)
    {
        _randomGenerator = randomGenerator;
        _fusionExperience = fusionExperience;
    }

    public PartnerFusionInfo GetPartnerFusionData(byte level, long percentage, int materialLevel, bool doubleExp)
    {
        PartnerFusionExperience experience = _fusionExperience.PartnerFusionExperiences.FirstOrDefault(s => s.LevelRange.Minimum <= level && s.LevelRange.Maximum >= level);

        if (experience == null)
        {
            return new PartnerFusionInfo { Level = level, Percentage = percentage };
        }
        
        int exp = 0;
        
        if (materialLevel > 1)
        {
            exp = (int)(materialLevel * 100 * _fusionExperience.ExperienceDivider);
        }

        exp += doubleExp ? experience.Experience * 2 : experience.Experience;

        percentage += exp;

        byte extraLevels = (byte)(percentage / 100);

        percentage -= extraLevels * 100;
        level += extraLevels;

        if (level < 100)
        {
            return new PartnerFusionInfo { Level = level, Percentage = percentage };
        }

        level = 100;
        percentage = 0;

        return new PartnerFusionInfo { Level = level, Percentage = percentage };
    }

    public SpPerfStats GetPartnerRandomStat(GameItemInstance partnerPsp)
    {
        SpPerfStats[] range =
        {
            SpPerfStats.Attack, SpPerfStats.Defense,
            SpPerfStats.CriticalDefence, SpPerfStats.HpMp,
            SpPerfStats.ResistanceFire, SpPerfStats.ResistanceWater,
            SpPerfStats.ResistanceLight, SpPerfStats.ResistanceDark
        };

        if (partnerPsp.SpDamage >= 100)
        {
            range = range.Except(new[] { SpPerfStats.Attack }).ToArray();
        }
        
        if (partnerPsp.SpDefence >= 100)
        {
            range = range.Except(new[] { SpPerfStats.Defense }).ToArray();
        }
        
        if (partnerPsp.SpCriticalDefense >= 100)
        {
            range = range.Except(new[] { SpPerfStats.CriticalDefence }).ToArray();
        }
        
        if (partnerPsp.SpHP >= 100)
        {
            range = range.Except(new[] { SpPerfStats.HpMp }).ToArray();
        }
        
        if (partnerPsp.SpFire >= 35)
        {
            range = range.Except(new[] { SpPerfStats.ResistanceFire }).ToArray();
        }
        
        if (partnerPsp.SpWater >= 35)
        {
            range = range.Except(new[] { SpPerfStats.ResistanceWater }).ToArray();
        }
        
        if (partnerPsp.SpLight >= 35)
        {
            range = range.Except(new[] { SpPerfStats.ResistanceLight }).ToArray();
        }
        
        if (partnerPsp.SpDark >= 35)
        {
            range = range.Except(new[] { SpPerfStats.ResistanceDark }).ToArray();
        }
        
        SpPerfStats stat = range[_randomGenerator.RandomNumber(range.Length)];

        return stat;
    }
}