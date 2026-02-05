using System.Collections.Concurrent;
using System.Collections.Generic;

namespace WingsEmu.Game.Configurations;

public interface IMonsterTalkingConfig
{
    bool HasPossibleMessages(int monsterVnum);
    IReadOnlyList<string> PossibleMessage(int monsterVnum);
}

public class MonsterTalkingConfig : IMonsterTalkingConfig
{
    private readonly ConcurrentDictionary<int, List<string>> _monsterMessage = new();
    private readonly HashSet<int> _monstersWithMessage = new();

    public MonsterTalkingConfig(IEnumerable<MonsterTalkingConfiguration> configurations)
    {
        foreach (MonsterTalkingConfiguration config in configurations)
        {
            if (config?.PossibleMessages == null || config.MobVnums == null)
            {
                continue;
            }

            foreach (int mobVnum in config.MobVnums)
            {
                _monstersWithMessage.Add(mobVnum);
                _monsterMessage.TryAdd(mobVnum, config.PossibleMessages);
            }
        }
    }

    public bool HasPossibleMessages(int monsterVnum) => _monstersWithMessage.Contains(monsterVnum);
    public IReadOnlyList<string> PossibleMessage(int monsterVnum) => _monsterMessage.TryGetValue(monsterVnum, out List<string> list) ? list : null;
}

public class MonsterTalkingConfiguration
{
    public List<string> PossibleMessages { get; set; }
    public List<int> MobVnums { get; set; }
}