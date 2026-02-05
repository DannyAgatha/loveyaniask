using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace WingsEmu.Plugins.Essentials.NPC;

public class BuffPack
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; }

    [YamlMember(Alias = "price")]
    public int Price { get; set; }

    [YamlMember(Alias = "minimum_level")]
    public int MinimumLevel { get; set; }

    [YamlMember(Alias = "maximum_level")]
    public int MaximumLevel { get; set; }

    [YamlMember(Alias = "buffs")]
    public List<BuffPackElement> Buffs { get; set; }
}