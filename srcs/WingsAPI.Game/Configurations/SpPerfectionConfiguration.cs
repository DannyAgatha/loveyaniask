using System.Collections.Generic;
using System.Runtime.Serialization;
using WingsEmu.Game._enum;

namespace WingsEmu.Game.Configurations;

[DataContract]
public class SpPerfectionConfiguration
{
    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public List<PerfUpgradeConfiguration> PerfUpgradeConfigurations { get; set; } = new()
    {
        new()
    };

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public Dictionary<SpPerfStats, short> StatProbabilityConfiguration { get; set; } = new()
    {
        { SpPerfStats.Attack, 10 },
        { SpPerfStats.Defense, 10 },
        { SpPerfStats.Element, 10 },
        { SpPerfStats.HpMp, 10 },
        { SpPerfStats.ResistanceFire, 10 },
        { SpPerfStats.ResistanceWater, 10 },
        { SpPerfStats.ResistanceLight, 10 },
        { SpPerfStats.ResistanceDark, 10 }
    };

    [DataMember(EmitDefaultValue = true, IsRequired = true)]
    public List<SpStoneLink> SpStonesLinks { get; set; } = new()
    {
        new()
        {
            StoneVnum = (int)ItemVnums.RUBY_COMP,
            SpVnums = new List<int>
            {
                (int)ItemVnums.GLADIATOR_SPECIALIST_CARD
            }
        }
    };
}