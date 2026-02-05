using System.Collections.Generic;
using System.Linq;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;

namespace WingsEmu.Game.Configurations;

public interface IMajorTrophyNpcRunCraftItemConfiguration
{
    MajorTrophyNpcRunCraftItemConfig GetConfigByNpcRun(NpcRunType npcRunType);
}

public class MajorTrophyNpcRunCraftItemConfiguration : IMajorTrophyNpcRunCraftItemConfiguration
{
    private readonly Dictionary<NpcRunType, MajorTrophyNpcRunCraftItemConfig> _MajorTrophyNpcRunItemConfigs;

    public MajorTrophyNpcRunCraftItemConfiguration(IEnumerable<MajorTrophyNpcRunCraftItemConfig> configs)
    {
        _MajorTrophyNpcRunItemConfigs = configs.Where(x => x?.NeededItems != null).ToDictionary(x => x.NpcRun);
    }

    public MajorTrophyNpcRunCraftItemConfig GetConfigByNpcRun(NpcRunType npcRunType) => _MajorTrophyNpcRunItemConfigs.GetValueOrDefault(npcRunType);
}

public class MajorTrophyNpcRunCraftItemConfig
{
    public NpcRunType NpcRun { get; set; }
    public int CraftedItem { get; set; }
    public int Amount { get; set; }
    public bool? ItemByClass { get; set; }
    public ClassType? ClassType { get; set; }
    public List<MajorTrophyNpcRunCraftItemConfigItem> NeededItems { get; set; }
}

public class MajorTrophyNpcRunCraftItemConfigItem
{
    public int Item { get; set; }
    public int Amount { get; set; }
}