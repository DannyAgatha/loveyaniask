using YamlDotNet.Serialization;

namespace NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Rewards;

public class LevelRewardsItem
{
    [YamlMember(Alias = "item_vnum", ApplyNamingConventions = true)]
    public int ItemVnum { get; set; }

    [YamlMember(Alias = "quantity", ApplyNamingConventions = true)]
    public ushort Quantity { get; set; }

    [YamlMember(Alias = "rarity", ApplyNamingConventions = true)]
    public byte Rarity { get; set; }

    [YamlMember(Alias = "upgrade", ApplyNamingConventions = true)]
    public byte Upgrade { get; set; }
}