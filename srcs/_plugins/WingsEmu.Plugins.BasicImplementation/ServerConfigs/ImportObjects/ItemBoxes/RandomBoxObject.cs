// NosEmu
// 


using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.ItemBoxes;

public class RandomBoxObject
{
    [YamlMember(Alias = "itemVnum", ApplyNamingConventions = true)]
    public int ItemVnum { get; set; }

    [YamlMember(Alias = "minimum_rewards", ApplyNamingConventions = true)]
    public int? MinimumRewards { get; set; }

    [YamlMember(Alias = "maximum_rewards", ApplyNamingConventions = true)]
    public int? MaximumRewards { get; set; }

    [YamlMember(Alias = "hideRewardInfo", ApplyNamingConventions = true)]
    public bool HideRewardInfo { get; set; }

    [YamlMember(Alias = "categories", ApplyNamingConventions = true)]
    public List<RandomBoxCategory> Categories { get; set; }

    [YamlMember(Alias = "sealed_event", ApplyNamingConventions = true)]
    public List<RandomBoxCategory> SealedEvent { get; set; }
}