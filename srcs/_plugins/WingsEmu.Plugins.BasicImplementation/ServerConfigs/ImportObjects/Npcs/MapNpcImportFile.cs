// NosEmu
// 


using System.Collections.Generic;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Files;
using YamlDotNet.Serialization;

namespace NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Npcs;

public class MapNpcImportFile : IFileData
{
    [YamlMember(Alias = "mapId", ApplyNamingConventions = true)]
    public int MapId { get; set; }

    [YamlMember(Alias = "npcs", ApplyNamingConventions = true)]
    public List<MapNpcObject> Npcs { get; set; }
}