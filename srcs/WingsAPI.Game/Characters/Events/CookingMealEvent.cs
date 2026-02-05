using System.Collections.Generic;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Characters.Events
{
    public class CookingMealEvent : PlayerEvent
    {
        public CookingMealEvent(int skillVnum, int recipeGeneratedItemVnum, int recipeGeneratedItemAmount, IReadOnlyList<Recipe> recipes) 
        { 
            SkillVnum = skillVnum; 
            RecipeGeneratedItemAmount = recipeGeneratedItemAmount;
            RecipeGeneratedItemVnum = recipeGeneratedItemVnum;
            Recipes = recipes;
        }

        public int SkillVnum { get; set; }
        public int RecipeGeneratedItemVnum { get; set; }
        public int RecipeGeneratedItemAmount { get; set; }
        public IReadOnlyList<Recipe> Recipes { get; set; }
    }
}