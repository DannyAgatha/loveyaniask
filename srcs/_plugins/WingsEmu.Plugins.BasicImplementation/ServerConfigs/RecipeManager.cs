using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Recipes;
using PhoenixLib.Caching;
using PhoenixLib.Logging;
using WingsEmu.DTOs.Recipes;
using WingsEmu.Game;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.ServerData;
using WingsEmu.Game.Managers.StaticData;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects;

namespace NosEmu.Plugins.BasicImplementations.ServerConfigs;

public class RecipeManager : IRecipeManager
{
    private readonly IEnumerable<RecipeImportFile> _files;
    private readonly HashSet<Recipe> _generalRecipes = new();
    private readonly IItemsManager _itemsManager;
    private readonly IRecipeFactory _recipeFactory;
    private readonly IKeyValueCache<List<Recipe>> _recipes;

    public RecipeManager(IEnumerable<RecipeImportFile> files, IRecipeFactory recipeFactory, IKeyValueCache<List<Recipe>> recipes, IItemsManager itemsManager)
    {
        _files = files;
        _recipes = recipes;
        _itemsManager = itemsManager;
        _recipeFactory = recipeFactory;
    }

    public async Task InitializeAsync()
    {
        var recipes = new List<RecipeDTO>();
        foreach (RecipeObject recipeObject in _files.SelectMany(x => x.Recipes))
        {
            if (recipeObject == null)
            {
                continue;
            }

            RecipeDTO recipe = recipeObject.ToDto();

            IGameItem producerItem = _itemsManager.GetItem(recipe.ProducedItemVnum);
            if (producerItem is null)
            {
                Log.Warn("[RECIPE] Item not found: " + recipe.Id +
                    $" on recipe ProducerItemVnum: {recipe.ProducerItemVnum} | ProducerNpc: {recipe.ProducerNpcVnum} | Producer: {recipe.ProducerMapNpcId}");
            }

            List<RecipeItemDTO> items = new();
            if (recipeObject.Items != null)
            {
                short slot = 0;
                foreach (RecipeItemObject recipeItem in recipeObject.Items)
                {
                    if (recipeItem == null)
                    {
                        continue;
                    }

                    IGameItem item = _itemsManager.GetItem(recipeItem.ItemVnum);
                    if (item is null)
                    {
                        Log.Warn("[RECIPE] Item not found: " + recipeItem.ItemVnum +
                            $" on recipe ProducerItemVnum: {recipe.ProducerItemVnum} | ProducerNpc: {recipe.ProducerNpcVnum} | Producer: {recipe.ProducerMapNpcId}");
                        continue;
                    }

                    items.Add(recipeItem.ToDto(slot));
                    slot++;
                }
            }

            recipe.Items = items;
            recipes.Add(recipe);
        }

        int count = 0;
        foreach (RecipeDTO recipe in recipes)
        {
            if (recipe.Items == null)
            {
                continue;
            }

            Recipe gameRecipe = _recipeFactory.CreateRecipe(recipe);
            _generalRecipes.Add(gameRecipe);
            count++;

            if (gameRecipe.ProducerItemVnum.HasValue)
            {
                string key = $"item-{gameRecipe.ProducerItemVnum.Value}";
                List<Recipe> list = _recipes.Get(key);
                if (list == null)
                {
                    list = new List<Recipe>();
                    _recipes.Set(key, list);
                }

                list.Add(gameRecipe);
                continue;
            }

            if (gameRecipe.ProducerNpcVnum.HasValue)
            {
                string key = $"npcVnum-{gameRecipe.ProducerNpcVnum.Value}";
                List<Recipe> list = _recipes.Get(key);
                if (list == null)
                {
                    list = new List<Recipe>();
                    _recipes.Set(key, list);
                }

                list.Add(gameRecipe);
                continue;
            }
            
            if (gameRecipe.ProducerSkillVnum.HasValue)
            {
                string key = $"skillVnum-{gameRecipe.ProducerSkillVnum.Value}";
                List<Recipe> list = _recipes.Get(key);
                if (list == null)
                {
                    list = new List<Recipe>();
                    _recipes.Set(key, list);
                }

                list.Add(gameRecipe);
                continue;
            }

            if (gameRecipe.ProducerMapNpcId.HasValue)
            {
                string key = $"mapNpc-{gameRecipe.ProducerMapNpcId.Value}";
                List<Recipe> list = _recipes.Get(key);
                if (list == null)
                {
                    list = new List<Recipe>();
                    _recipes.Set(key, list);
                }

                list.Add(gameRecipe);
            }
        }

        Log.Info($"[RECIPE_MANAGER] Loaded {count.ToString()} recipes");
    }

    public IReadOnlyList<Recipe> GetRecipesByProducerItemVnum(int itemVnum) => _recipes.Get($"item-{itemVnum}");
    public IReadOnlyList<Recipe> GetRecipesByNpcId(long mapNpcId) => _recipes.Get($"mapNpc-{mapNpcId}");
    public IReadOnlyList<Recipe> GetRecipesByNpcMonsterVnum(int npcVNum) => _recipes.Get($"npcVnum-{npcVNum}");
    public IReadOnlyList<Recipe> GetRecipesBySkillnum(int skillVNum) => _recipes.Get($"skillVnum-{skillVNum}");
    public IReadOnlyList<Recipe> GetRecipeByProducedItemVnum(int itemVnum) => _generalRecipes.Where(x => x != null && x.ProducedItemVnum == itemVnum).ToList();
}