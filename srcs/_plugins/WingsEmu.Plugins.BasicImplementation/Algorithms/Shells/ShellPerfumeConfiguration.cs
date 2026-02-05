// NosEmu
// 


using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NosEmu.Plugins.BasicImplementations.Algorithms.Shells;

public interface IShellPerfumeConfiguration
{
    int? GetPerfumesByLevelAndRarity(short level, byte rarity, bool isHeroic);
    int? GetGoldByLevel(short level, bool isHero);
}

public class ShellPerfumeConfiguration : IShellPerfumeConfiguration
{
    private readonly ImmutableList<PerfumeConfiguration> _perfumes;

    public ShellPerfumeConfiguration(IEnumerable<PerfumeConfiguration> perfumeConfigurations) => _perfumes = perfumeConfigurations.ToImmutableList();

    public int? GetPerfumesByLevelAndRarity(short level, byte rarity, bool isHeroic)
    {
        PerfumeConfiguration perfumeConfiguration = _perfumes.FirstOrDefault(s => s.IsHero == isHeroic && s.MinLevel <= level && s.MaxLevel >= level);
        if (perfumeConfiguration == null)
        {
            return null;
        }

        int? perfumes = perfumeConfiguration.Perfumes.FirstOrDefault(s => s.Rarity == rarity)?.Perfumes;
        return perfumes ?? 0;
    }

    public int? GetGoldByLevel(short level, bool isHeroic) => _perfumes
        .FirstOrDefault(s => s.IsHero == isHeroic && s.MinLevel <= level && s.MaxLevel >= level)?.Gold;
}

public class PerfumeConfiguration
{
    public int MinLevel { get; set; }
    public int MaxLevel { get; set; }
    public int Gold { get; set; }
    public bool IsHero { get; set; }
    public List<PerfumesByRarity> Perfumes { get; set; }
}

public class PerfumesByRarity
{
    public int Rarity { get; set; }
    public int Perfumes { get; set; }
}