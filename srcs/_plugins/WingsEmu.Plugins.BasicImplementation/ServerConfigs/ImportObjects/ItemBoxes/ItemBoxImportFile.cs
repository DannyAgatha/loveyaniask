using System.Collections.Generic;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Files;
using YamlDotNet.Serialization;

namespace NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.ItemBoxes;

public class ItemBoxImportFile : IFileData
{
    [YamlMember(Alias = "item_vnum", ApplyNamingConventions = true)]
    public int ItemVnum { get; set; }

    [YamlMember(Alias = "minimum_rewards", ApplyNamingConventions = true)]
    public int? MinimumRewards { get; set; }

    [YamlMember(Alias = "maximum_rewards", ApplyNamingConventions = true)]
    public int? MaximumRewards { get; set; }

    [YamlMember(Alias = "show_raidbox_modal_on_open", ApplyNamingConventions = true)]
    public bool ShowRaidBoxModalOnOpen { get; set; }

    [YamlMember(Alias = "items", ApplyNamingConventions = true)]
    public List<RandomBoxItem> Items { get; set; }
}