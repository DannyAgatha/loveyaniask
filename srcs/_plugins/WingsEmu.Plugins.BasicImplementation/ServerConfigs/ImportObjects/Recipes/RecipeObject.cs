// NosEmu
// 


using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Recipes;

public class RecipeObject
{
    [YamlMember(Alias = "itemVnum", ApplyNamingConventions = true)]
    public int ItemVnum { get; set; }

    [YamlMember(Alias = "quantity", ApplyNamingConventions = true)]
    public int Quantity { get; set; }

    [YamlMember(Alias = "producerItemVnum", ApplyNamingConventions = true)]
    public int? ProducerItemVnum { get; set; }

    [YamlMember(Alias = "producerNpcVnum", ApplyNamingConventions = true)]
    public int? ProducerNpcVnum { get; set; }

    [YamlMember(Alias = "producerMapNpcId", ApplyNamingConventions = true)]
    public int? ProducerMapNpcId { get; set; }
    
    [YamlMember(Alias = "producerSkillVnum", ApplyNamingConventions = true)]
    public int? ProducerSkillVnum { get; set; }

    [YamlMember(Alias = "producedChefXp", ApplyNamingConventions = true)]
    public int? ProducedChefXp { get; set; }

    [YamlMember(Alias = "limitCrafting", ApplyNamingConventions = true)]
    public int? LimitCrafting { get; set; }

    [YamlMember(Alias = "bearingChef", ApplyNamingConventions = true)]
    public byte? BearingChef { get; set; }

    [YamlMember(Alias = "items", ApplyNamingConventions = true)]
    public List<RecipeItemObject> Items { get; set; }
}