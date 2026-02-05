using System.Collections.Generic;
using WingsEmu.Packets.Enums;
using YamlDotNet.Serialization;

namespace NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Rewards;

public class LevelRewardsObject
{
    [YamlMember(Alias = "level_type", ApplyNamingConventions = true)]
    public LevelType LevelType { get; set; }

    [YamlMember(Alias = "level_value", ApplyNamingConventions = true)]
    public byte LevelValue { get; set; }

    [YamlMember(Alias = "items_rewards", ApplyNamingConventions = true)]
    public List<LevelRewardsItem> ItemsRewards { get; set; }
}