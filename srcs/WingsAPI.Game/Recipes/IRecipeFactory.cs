using WingsEmu.DTOs.Recipes;

namespace WingsEmu.Game;

public interface IRecipeFactory
{
    Recipe CreateRecipe(RecipeDTO recipeDto);
}