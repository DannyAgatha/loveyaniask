using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.MinilandExtensions;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Configurations.Miniland;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Miniland.Events;
using WingsEmu.Game.Warehouse;
using WingsEmu.Game.Warehouse.Events;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Event.Miniland;

public class UseObjMinilandEventHandler : IAsyncEventProcessor<UseObjMinilandEvent>
{
    private const MinigameInteraction ThisAction = MinigameInteraction.GetMinigameInformation;
    private readonly IAccountWarehouseManager _accountWarehouseManager;
    private readonly IGameLanguageService _languageService;
    private readonly MinigameConfiguration _minigameConfiguration;
    private readonly IMinigameManager _minigameManager;

    public UseObjMinilandEventHandler(MinigameConfiguration minigameConfiguration, IMinigameManager minigameManager, IGameLanguageService languageService,
        IAccountWarehouseManager accountWarehouseManager)
    {
        _minigameConfiguration = minigameConfiguration;
        _minigameManager = minigameManager;
        _languageService = languageService;
        _accountWarehouseManager = accountWarehouseManager;
    }

    public async Task HandleAsync(UseObjMinilandEvent e, CancellationToken cancellation)
    {
        if (e.Sender.CurrentMapInstance.MapInstanceType != MapInstanceType.Miniland)
        {
            return;
        }

        MapDesignObject mapObject = e.Sender.CurrentMapInstance.MapDesignObjects.FirstOrDefault(x => x.InventoryItem.Slot == e.Slot);
        if (mapObject == default)
        {
            return;
        }


        switch (mapObject.InventoryItem.ItemInstance.GameItem.ItemType)
        {
            // last data = miniland chest
            case ItemType.Minigame when mapObject.InventoryItem.ItemInstance.GameItem.Data[^1] == 1: // miniland chest flag
                if (e.Sender.PlayerEntity.Miniland.Id != e.Sender.CurrentMapInstance.Id)
                {
                    // not the owner of the miniland
                    return;
                }

                e.Sender.EmitEvent(new MinilandChestViewContentEvent(mapObject.InventoryItem.ItemInstance.ItemVNum));
                break;

            // last data = crafting structures
            case ItemType.Terrace when mapObject.InventoryItem.ItemInstance.GameItem.Data[^1] == 2:
            case ItemType.Garden when mapObject.InventoryItem.ItemInstance.GameItem.Data[^1] == 2:
            case ItemType.Minigame when mapObject.InventoryItem.ItemInstance.GameItem.Data[^1] == 2: // crafting structures
                if (e.Sender.PlayerEntity.Miniland.Id != e.Sender.CurrentMapInstance.Id)
                {
                    // not the owner of the miniland
                    return;
                }

                e.Sender.EmitEvent(new RecipeOpenWindowEvent(mapObject.InventoryItem.ItemInstance.ItemVNum));
                break;
            case ItemType.Minigame:
                MinilandInteractionInformationHolder lastMinilandInteraction = _minigameManager.GetLastInteraction(e.Sender);

                if (lastMinilandInteraction.Interaction != MinigameInteraction.None
                    && lastMinilandInteraction.Interaction != MinigameInteraction.GetMinigameInformation
                    && lastMinilandInteraction.Interaction != MinigameInteraction.DeclarateStop
                    && lastMinilandInteraction.Interaction != MinigameInteraction.GetMinigameDurability
                    && lastMinilandInteraction.Interaction != MinigameInteraction.GetYieldInformation
                    && lastMinilandInteraction.Interaction != MinigameInteraction.GetYieldReward
                    && lastMinilandInteraction.Interaction != MinigameInteraction.UseDurabilityCoupon
                    && lastMinilandInteraction.Interaction != MinigameInteraction.RepairMinigameDurability
                    && !((lastMinilandInteraction.Interaction == MinigameInteraction.DeclaratePlay || lastMinilandInteraction.Interaction == MinigameInteraction.GetReward)
                        && lastMinilandInteraction.MapObject.Id == mapObject.Id))
                {
                    _minigameManager.ReportInteractionIncoherence(e.Sender, lastMinilandInteraction.Interaction, lastMinilandInteraction.MapObject, ThisAction, mapObject);
                }

                Minigame minigameConfig = _minigameManager.GetSpecificMinigameConfiguration(mapObject.InventoryItem.ItemInstance.ItemVNum);
                if (e.Sender.PlayerEntity.Level < minigameConfig.MinimumLevel
                    || e.Sender.PlayerEntity.Reput < minigameConfig.MinimumReputation)
                {
                    e.Sender.SendErrorChatMessage(string.Format(
                        _languageService.GetLanguage(GameDialogKey.MINILAND_CHATMESSAGE_NOT_FULLFILLING_MINIGAME_REQUIREMENTS, e.Sender.UserLanguage),
                        minigameConfig.MinimumLevel.ToString(),
                        minigameConfig.MinimumReputation.ToString()));
                    return;
                }

                _minigameManager.RegisterInteraction(e.Sender, new MinilandInteractionInformationHolder(ThisAction, mapObject));
                e.Sender.SendMinigameInfo(mapObject, _minigameConfiguration, _minigameManager.GetScores(mapObject.InventoryItem.ItemInstance.ItemVNum));
                break;
            case ItemType.House when mapObject.InventoryItem.ItemInstance.GameItem.IsWarehouse:
                await e.Sender.EmitEventAsync(new AccountWarehouseOpenEvent());
                break;
        }
    }
}