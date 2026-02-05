// NosEmu
// 


using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsEmu.Game;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.ServerData;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Etc;

public class ProduceItemHandler : IItemHandler
{
    private readonly IItemUsageManager _itemUsageManager;
    private readonly IRecipeManager _recipeManager;

    public ProduceItemHandler(IRecipeManager recipeManager, IItemUsageManager itemUsageManager)
    {
        _recipeManager = recipeManager;
        _itemUsageManager = itemUsageManager;
    }

    public ItemType ItemType => ItemType.Production;
    public long[] Effects => new long[] { 100 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (session.PlayerEntity.IsCraftingItem)
        {
            return;
        }

        if (session.PlayerEntity.IsOnVehicle)
        {
            return;
        }

        session.PlayerEntity.LastNRunId = 0;
        _itemUsageManager.SetLastItemUsed(session.PlayerEntity.Id, e.Item.ItemInstance.GameItem.Id);
        IReadOnlyList<Recipe> recipeList = _recipeManager.GetRecipesByProducerItemVnum(e.Item.ItemInstance.GameItem.Id);

        if (recipeList == null || !recipeList.Any())
        {
            Log.Debug($"No Recipe Found: {ItemType}|{e.Item.ItemInstance.GameItem.ItemSubType}|{e.Item.ItemInstance.GameItem.Effect}|{e.Item.ItemInstance.GameItem.EffectValue}");
            return;
        }

        session.SendWopenPacket((byte)WindowType.CRAFTING_ITEMS, e.Item.Slot, 0);

        if (e.Item.Slot == 0) // Entwell :pepega:
        {
            session.PlayerEntity.IsCraftingItem = false;
        }

        session.SendRecipeItemList(recipeList, e.Item.ItemInstance.GameItem);
        session.SendInventoryAddPacket(e.Item);
    }
}