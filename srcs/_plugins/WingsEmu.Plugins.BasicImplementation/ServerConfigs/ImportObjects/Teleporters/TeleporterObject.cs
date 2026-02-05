using WingsEmu.Packets.Enums;
using YamlDotNet.Serialization;

namespace NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Teleporters;

public class TeleporterObject
{
    [YamlMember(Alias = "index", ApplyNamingConventions = true)]
    public short Index { get; set; }

    [YamlMember(Alias = "type", ApplyNamingConventions = true)]
    public TeleporterType Type { get; set; }

    [YamlIgnore]
    public int MapId { get; set; }

    [YamlMember(Alias = "mapNpcId", ApplyNamingConventions = true)]
    public int MapNpcId { get; set; }

    [YamlMember(Alias = "mapX", ApplyNamingConventions = true)]
    public short MapX { get; set; }

    [YamlMember(Alias = "mapY", ApplyNamingConventions = true)]
    public short MapY { get; set; }
}