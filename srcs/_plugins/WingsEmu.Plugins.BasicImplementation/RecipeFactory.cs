using System.Collections.Generic;
using WingsEmu.DTOs.Recipes;
using WingsEmu.Game;

namespace NosEmu.Plugins.BasicImplementations;

public class RecipeFactory : IRecipeFactory
{
    private readonly List<RecipeItemDTO> _emptyItemDtos = new();

    public Recipe CreateRecipe(RecipeDTO recipeDto)
    {
        return new Recipe(recipeDto.Id, recipeDto.Amount, recipeDto.ProducerMapNpcId, recipeDto.ProducerItemVnum, recipeDto.ProducerNpcVnum,
            recipeDto.ProducedItemVnum, recipeDto.Items ?? _emptyItemDtos, recipeDto.ProducerSkillVnum, recipeDto.ProducedChefXp, recipeDto.LimitCrafting, recipeDto.BearingChef);
    }
}