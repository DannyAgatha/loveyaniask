using System.Collections.Generic;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Files;
using YamlDotNet.Serialization;

namespace NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Teleporters;

public class TeleporterImportFile : IFileData
{
    [YamlMember(Alias = "mapId", ApplyNamingConventions = true)]
    public int MapId { get; set; }

    [YamlMember(Alias = "teleporters", ApplyNamingConventions = true)]
    public List<TeleporterObject> Teleporters { get; set; }
}