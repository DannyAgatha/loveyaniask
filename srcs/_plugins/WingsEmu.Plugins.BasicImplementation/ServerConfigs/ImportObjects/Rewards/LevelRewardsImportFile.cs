using System.Collections.Generic;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Files;
using WingsEmu.Packets.Enums.Character;
using YamlDotNet.Serialization;

namespace NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Rewards;

public class LevelRewardsImportFile : IFileData
{
    [YamlMember(Alias = "class_type", ApplyNamingConventions = true)]
    public ClassType ClassType { get; set; }

    [YamlMember(Alias = "level_rewards", ApplyNamingConventions = true)]
    public List<LevelRewardsObject> Items { get; set; }
}