using YamlDotNet.Serialization;

namespace NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.ItemBoxes;

public class RandomBoxItem
{
    [YamlMember(Alias = "itemVnum", ApplyNamingConventions = true)]
    public int ItemVnum { get; set; }

    [YamlMember(Alias = "quantity", ApplyNamingConventions = true)]
    public ushort Quantity { get; set; }

    [YamlMember(Alias = "upgrade", ApplyNamingConventions = true)]
    public byte Upgrade { get; set; }
    
    [YamlMember(Alias = "rarity", ApplyNamingConventions = true)]
    public sbyte Rarity { get; set; }

    [YamlMember(Alias = "randomRarity", ApplyNamingConventions = true)]
    public bool RandomRarity { get; set; }

    [YamlMember(Alias = "isSuperReward", ApplyNamingConventions = true)]
    public bool IsSuperReward { get; set; }

    [YamlMember(Alias = "minimumRandomRarity", ApplyNamingConventions = true)]
    public short MinimumRandomRarity { get; set; }

    [YamlMember(Alias = "maximumRandomRarity", ApplyNamingConventions = true)]
    public short MaximumRandomRarity { get; set; }

    [YamlMember(Alias = "message", ApplyNamingConventions = true)]
    public string Message { get; set; }
}