using System.Collections.Generic;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Files;
using YamlDotNet.Serialization;

namespace NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.ItemBoxes;

public class RandomBoxImportFile : IFileData
{
    [YamlMember(Alias = "randomBoxes", ApplyNamingConventions = true)]
    public List<RandomBoxObject> Items { get; set; }
}