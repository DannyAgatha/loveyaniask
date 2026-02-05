using System.Collections.Generic;
using WingsEmu.DTOs.Maps;
using YamlDotNet.Serialization;

namespace NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Maps;

public class ConfiguredMapObject
{
    [YamlMember(Alias = "map_id", ApplyNamingConventions = true)]
    public int MapId { get; set; }

    [YamlMember(Alias = "map_vnum", ApplyNamingConventions = true)]
    public int MapVnum { get; set; }

    [YamlMember(Alias = "map_name_id", ApplyNamingConventions = true)]
    public int NameId { get; set; }

    [YamlMember(Alias = "map_music_id", ApplyNamingConventions = true)]
    public int MusicId { get; set; }

    [YamlMember(Alias = "map_ambient_id", ApplyNamingConventions = true)]
    public int AmbientId { get; set; }

    [YamlMember(Alias = "flags", ApplyNamingConventions = true)]
    public List<MapFlags> Flags { get; set; }
}