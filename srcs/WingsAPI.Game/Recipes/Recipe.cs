// NosEmu
// 


using System.Collections.Generic;
using WingsEmu.DTOs.Recipes;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game;

public class Recipe
{
    public Recipe(int id, int amount, int? producerMapNpcId, int? producerItemVnum, int? producerNpcVnum, int producedItemVnum, IReadOnlyList<RecipeItemDTO> items, 
        int? producerSkillVnum, int? producedChefXp, int? limitCrafting, byte? bearingChef)
    {
        Id = id;
        Amount = amount;
        ProducerMapNpcId = producerMapNpcId;
        ProducerItemVnum = producerItemVnum;
        ProducerNpcVnum = producerNpcVnum;
        ProducedItemVnum = producedItemVnum;
        Items = items;
        ProducerSkillVnum = producerSkillVnum;
        ProducedChefXp = producedChefXp;
        LimitCrafting = limitCrafting;
        BearingChef = bearingChef;
    }

    public int Id { get; }
    public int Amount { get; }
    public int? ProducerMapNpcId { get; }
    public int? ProducerItemVnum { get; }
    public int? ProducerNpcVnum { get; }
    public int ProducedItemVnum { get; }
    public int? ProducerSkillVnum { get; }
    public int? ProducedChefXp { get; }
    public int? LimitCrafting { get; }
    
    public byte? BearingChef { get; }
    public IReadOnlyList<RecipeItemDTO> Items { get; }
}

public class RecipeOpenWindowEvent : PlayerEvent
{
    public RecipeOpenWindowEvent(int itemVnum) => ItemVnum = itemVnum;

    public int ItemVnum { get; }
}