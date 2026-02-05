using System.Collections.Generic;
using System.Linq;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Configurations;

public interface IAct5NpcRunCraftItemConfiguration
{
    Act5NpcRunCraftItemConfig GetConfigByNpcRun(NpcRunType npcRunType);
}

public class Act5NpcRunCraftItemConfiguration : IAct5NpcRunCraftItemConfiguration
{
    private readonly Dictionary<NpcRunType, Act5NpcRunCraftItemConfig> _act5NpcRunItemConfigs;

    public Act5NpcRunCraftItemConfiguration(IEnumerable<Act5NpcRunCraftItemConfig> configs)
    {
        _act5NpcRunItemConfigs = configs.Where(x => x?.NeededItems != null).ToDictionary(x => x.NpcRun);
    }

    public Act5NpcRunCraftItemConfig GetConfigByNpcRun(NpcRunType npcRunType) => _act5NpcRunItemConfigs.GetValueOrDefault(npcRunType);
}

public class Act5NpcRunCraftItemConfig
{
    public NpcRunType NpcRun { get; set; }
    public int CraftedItem { get; set; }
    public int Amount { get; set; }
    public bool? ItemByClass { get; set; }
    public List<Act5NpcRunCraftItemConfigItem> NeededItems { get; set; }
}

public class Act5NpcRunCraftItemConfigItem
{
    public int Item { get; set; }
    public int Amount { get; set; }
}