using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using WingsEmu.Core.Extensions;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Configurations;

public interface INpcRunTypeQuestsConfiguration
{
    List<int> GetPossibleQuestsByNpcRunType(NpcRunType npcRunType);
    bool HaveTheSameNpcRunType(int firstQuestVnum, int secondQuestVnum);
}

public class NpcRunTypeQuestsConfiguration : INpcRunTypeQuestsConfiguration
{
    private readonly ImmutableDictionary<NpcRunType, NpcRunTypeQuestsInfo> _questsByNpcRunType;

    public NpcRunTypeQuestsConfiguration(IEnumerable<NpcRunTypeQuestsInfo> questsByNpcRunType)
    {
        _questsByNpcRunType = questsByNpcRunType.ToImmutableDictionary(s => s.NpcRunType);
    }

    public List<int> GetPossibleQuestsByNpcRunType(NpcRunType npcRunType) => _questsByNpcRunType.ContainsKey(npcRunType)
        ? _questsByNpcRunType.GetOrDefault(npcRunType).PossibleQuests
        : new List<int>();

    public bool HaveTheSameNpcRunType(int firstQuestVnum, int secondQuestVnum) =>
        _questsByNpcRunType.Any(s => s.Value.PossibleQuests.Contains(firstQuestVnum) && s.Value.PossibleQuests.Contains(secondQuestVnum));
}

public class NpcRunTypeQuestsInfo
{
    public NpcRunType NpcRunType { get; set; }
    public List<int> PossibleQuests { get; set; }
}