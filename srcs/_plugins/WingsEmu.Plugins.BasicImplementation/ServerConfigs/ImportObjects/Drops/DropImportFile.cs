// NosEmu
// 


using System.Collections.Generic;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Files;
using YamlDotNet.Serialization;

namespace NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Drops;

public class DropImportFile : IFileData
{
    [YamlMember(Alias = "drops", ApplyNamingConventions = true)]
    public List<DropObject> Drops { get; set; }
}