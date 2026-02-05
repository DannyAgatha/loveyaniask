using System.Collections.Generic;
using System.Linq;
using WingsEmu.Core.Extensions;

namespace WingsEmu.Game._ItemUsage.Configuration;

public interface ISpWingConfiguration
{
    SpWingInfo GetSpWingInfo(int wingType);
}

public class SpWingConfiguration : ISpWingConfiguration
{
    private readonly SpWingInfoConfiguration _conf;

    public SpWingConfiguration(SpWingInfoConfiguration conf) => _conf = conf;

    public SpWingInfo GetSpWingInfo(int wingType) => _conf.GetOrDefault(wingType);
}

public class MateBuffConfigsContainer : IMateBuffConfigsContainer
{
    private readonly Dictionary<int, MateBuffIndividualConfig> _conf;

    public MateBuffConfigsContainer(MateBuffConfiguration conf)
    {
        _conf = conf.ToDictionary(s => s.PetVnum, s => s);
    }

    public MateBuffIndividualConfig GetMateBuffInfo(int mateVnum) => _conf.GetOrDefault(mateVnum);
}

public interface IMateBuffConfigsContainer
{
    MateBuffIndividualConfig GetMateBuffInfo(int mateVnum);
}

public class MateBuffConfiguration : List<MateBuffIndividualConfig>
{
}

public class MateBuffIndividualConfig
{
    public int PetVnum { get; set; }
    public List<int> BuffIds { get; set; }
}

public class SpWingInfoConfiguration : Dictionary<int, SpWingInfo>
{
}

public class SpWingInfo
{
    public List<WingBuff> Buffs { get; set; }
}

public class WingBuff
{
    public int BuffId { get; set; }
    public bool IsPermanent { get; set; }
}