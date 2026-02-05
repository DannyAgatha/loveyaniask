using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Drops;

public class DropObject
{
    [YamlMember(Alias = "chance", ApplyNamingConventions = true)]
    public int Chance { get; set; }

    [YamlMember(Alias = "itemVnum", ApplyNamingConventions = true)]
    public int ItemVNum { get; set; }

    [YamlMember(Alias = "quantity", ApplyNamingConventions = true)]
    public int Quantity { get; set; }

    [YamlMember(Alias = "races", ApplyNamingConventions = true)]
    public List<int[]> Races { get; set; }

    [YamlMember(Alias = "mapIds", ApplyNamingConventions = true)]
    public int[] MapIds { get; set; }

    [YamlMember(Alias = "monsterVnums", ApplyNamingConventions = true)]
    public int[] MonsterVnums { get; set; }
}