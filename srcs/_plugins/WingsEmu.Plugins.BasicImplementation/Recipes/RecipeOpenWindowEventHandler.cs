// NosEmu
// 


using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsEmu.Game;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.ServerData;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Recipes;

public class RecipeOpenWindowEventHandler : IAsyncEventProcessor<RecipeOpenWindowEvent>
{
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IItemsManager _itemsManager;
    private readonly IItemUsageManager _itemUsageManager;
    private readonly IRecipeManager _recipeManager;

    public RecipeOpenWindowEventHandler(IRecipeManager recipeManager, IItemsManager itemsManager, IItemUsageManager itemUsageManager, IGameItemInstanceFactory gameItemInstanceFactory)
    {
        _recipeManager = recipeManager;
        _itemsManager = itemsManager;
        _itemUsageManager = itemUsageManager;
        _gameItemInstanceFactory = gameItemInstanceFactory;
    }

    public async Task HandleAsync(RecipeOpenWindowEvent e, CancellationToken cancellation)
    {
        IReadOnlyList<Recipe> recipes = _recipeManager.GetRecipesByProducerItemVnum(e.ItemVnum);
        if (recipes.Count == 0)
        {
            return;
        }

        IGameItem item = _itemsManager.GetItem(e.ItemVnum);
        if (item == null)
        {
            return;
        }

        IClientSession session = e.Sender;

        // Blowa: This is dirty, we know that. It's a fucking trick
        InventoryItem inventory = _gameItemInstanceFactory.CreateInventoryItem(item.Id);
        inventory.Slot = short.MaxValue;

        session.SendWopenPacket((byte)WindowType.CRAFTING_ITEMS, short.MaxValue, 0);
        session.SendRecipeItemList(recipes, item);
        session.SendInventoryAddPacket(inventory);
        session.PlayerEntity.LastMinilandProducedItem = item.Id;
        _itemUsageManager.SetLastItemUsed(session.PlayerEntity.Id, e.ItemVnum);
    }
}