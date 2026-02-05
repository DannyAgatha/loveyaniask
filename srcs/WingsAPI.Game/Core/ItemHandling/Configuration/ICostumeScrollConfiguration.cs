using System.Collections.Generic;
using System.Linq;
using WingsEmu.Core.Extensions;

namespace WingsEmu.Game._ItemUsage.Configuration;

public interface ICostumeScrollConfiguration
{
    IReadOnlyList<short> GetScrollMorphs(short scrollId);
}

public class CostumeScrollConfiguration : ICostumeScrollConfiguration
{
    private readonly Dictionary<int, CostumeMorph> _morphs;

    public CostumeScrollConfiguration(CostumeMorphFileConfiguration morphs)
    {
        _morphs = morphs.ToDictionary(s => s.ScrollItemVnum);
    }

    public IReadOnlyList<short> GetScrollMorphs(short scrollId)
    {
        CostumeMorph tmp = _morphs.GetOrDefault(scrollId);
        return tmp?.PossibleMorphs;
    }
}

public class CostumeMorphFileConfiguration : List<CostumeMorph>
{
}

public class CostumeMorph
{
    public int ScrollItemVnum { get; set; }
    public List<short> PossibleMorphs { get; set; }
}