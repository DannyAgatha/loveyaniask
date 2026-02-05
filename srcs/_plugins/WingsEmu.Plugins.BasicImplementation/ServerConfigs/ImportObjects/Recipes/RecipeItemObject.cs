using YamlDotNet.Serialization;

namespace NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Recipes;

public class RecipeItemObject
{
    [YamlMember(Alias = "itemVnum", ApplyNamingConventions = true)]
    public short ItemVnum { get; set; }

    [YamlMember(Alias = "quantity", ApplyNamingConventions = true)]
    public short Quantity { get; set; }
}