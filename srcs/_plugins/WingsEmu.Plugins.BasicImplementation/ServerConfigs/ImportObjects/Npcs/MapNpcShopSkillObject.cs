using YamlDotNet.Serialization;

namespace NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Npcs;

public class MapNpcShopSkillObject
{
    [YamlMember(Alias = "skillVnum", ApplyNamingConventions = true)]
    public short SkillVnum { get; set; }
}