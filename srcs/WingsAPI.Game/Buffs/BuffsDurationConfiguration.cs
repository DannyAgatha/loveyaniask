using System.Collections.Generic;
using System.Collections.Immutable;

namespace WingsEmu.Game.Buffs;

public interface IBuffsDurationConfiguration
{
    BuffDuration GetBuffDurationById(int id);
}

public class BuffsDurationConfiguration : IBuffsDurationConfiguration
{
    private readonly ImmutableDictionary<int, BuffDuration> _buffDurations;

    public BuffsDurationConfiguration(IEnumerable<BuffDuration> buffDurations)
    {
        _buffDurations = buffDurations.ToImmutableDictionary(s => s.BuffVnum);
    }

    public BuffDuration GetBuffDurationById(int id) => _buffDurations.GetValueOrDefault(id);
}

public class BuffDuration
{
    public int BuffVnum { get; set; }
    public bool IsPermanent { get; set; }
    public int MinDuration { get; set; }
    public int MaxDuration { get; set; }
}