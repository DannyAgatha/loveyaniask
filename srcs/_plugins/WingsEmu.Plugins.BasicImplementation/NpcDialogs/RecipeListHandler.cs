using System.Collections.Generic;
using System.Threading.Tasks;
using WingsEmu.Game;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers.ServerData;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs;

public class RecipeListHandler : INpcDialogAsyncHandler
{
    private readonly IRecipeManager _recipeManager;

    public RecipeListHandler(IRecipeManager recipeManager) => _recipeManager = recipeManager;

    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.OPEN_CRAFTING };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {

        if (session.PlayerEntity.HasShopOpened || session.PlayerEntity.IsInExchange())
            return;
        INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(e.NpcId);
        if (npcEntity == null)
        {
            return;
        }

        IReadOnlyList<Recipe> recipes = _recipeManager.GetRecipesByNpcId(e.NpcId) ?? _recipeManager.GetRecipesByNpcMonsterVnum(npcEntity.NpcVNum);
        if (recipes == null)
        {
            return;
        }

        session.SendWopenPacket((byte)WindowType.CRAFTING_RANDOM_ITEMS_RARITY);
        session.SendRecipeNpcList(recipes);
    }
}