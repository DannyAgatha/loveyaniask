using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace WingsEmu.Game.Configurations;

public interface IGibberishConfig
{
    IReadOnlyList<string> GetKeysById(int id);
}

public class GibberishConfig : IGibberishConfig
{
    private readonly ConcurrentDictionary<int, IReadOnlyList<string>> _keys = new();

    public GibberishConfig(IEnumerable<GibberishConfiguration> configuration)
    {
        foreach (GibberishConfiguration config in configuration)
        {
            _keys.TryAdd(config.Id, config.Keys ?? new List<string>());
        }
    }

    public IReadOnlyList<string> GetKeysById(int id) => _keys.TryGetValue(id, out IReadOnlyList<string> list) ? list : Array.Empty<string>();
}

public class GibberishConfiguration
{
    public int Id { get; set; }
    public List<string> Keys { get; set; }
}