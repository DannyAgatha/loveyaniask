using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.MinilandExtensions;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations.Miniland;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Miniland.Events;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums.Language;

namespace NosEmu.Plugins.BasicImplementations.Event.Miniland;

public class MinigameRepairDurabilityEventHandler : IAsyncEventProcessor<MinigameRepairDurabilityEvent>
{
    private const MinigameInteraction ThisAction = MinigameInteraction.RepairMinigameDurability;
    private readonly IGameLanguageService _languageService;
    private readonly MinigameConfiguration _minigameConfiguration;
    private readonly IMinigameManager _minigameManager;

    public MinigameRepairDurabilityEventHandler(MinigameConfiguration minigameConfiguration, IGameLanguageService languageService, IMinigameManager minigameManager)
    {
        _minigameConfiguration = minigameConfiguration;
        _languageService = languageService;
        _minigameManager = minigameManager;
    }

    public async Task HandleAsync(MinigameRepairDurabilityEvent e, CancellationToken cancellation)
    {
        MinilandInteractionInformationHolder minilandInteraction = _minigameManager.GetLastInteraction(e.Sender);

        if (minilandInteraction.Interaction != MinigameInteraction.GetMinigameDurability
            && minilandInteraction.Interaction != ThisAction
            && minilandInteraction.MapObject != e.MapObject)
        {
            _minigameManager.ReportInteractionIncoherence(e.Sender, minilandInteraction.Interaction, minilandInteraction.MapObject, ThisAction, e.MapObject);
            return;
        }

        if (e.MapObject.CharacterId != e.Sender.PlayerEntity.Id)
        {
            await e.Sender.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "Tried to repair a minigame that he doesn't own." +
                $" 'SuspectCharacterId': {e.Sender.PlayerEntity.Id.ToString()} | 'VictimCharacterId': {e.MapObject.CharacterId.ToString()}");
            return;
        }

        bool applyGoldCost = _minigameConfiguration.Configuration.RepairDurabilityGoldCost > 0;

        if (applyGoldCost
            && e.GoldToExpend < _minigameConfiguration.Configuration.RepairDurabilityGoldCost)
        {
            return;
        }

        long durabilityToRepair = e.GoldToExpend / _minigameConfiguration.Configuration.RepairDurabilityGoldCost;

        if (e.MapObject.InventoryItem.ItemInstance.DurabilityPoint + durabilityToRepair > e.MapObject.InventoryItem.ItemInstance.GameItem.MinilandObjectPoint)
        {
            return;
        }

        if (!e.Sender.PlayerEntity.RemoveGold(durabilityToRepair * _minigameConfiguration.Configuration.RepairDurabilityGoldCost))
        {
            e.Sender.SendInfoI(Game18NConstString.NotEnoughGold5);
            return;
        }

        _minigameManager.RegisterInteraction(e.Sender, new MinilandInteractionInformationHolder(ThisAction, e.MapObject));
        e.MapObject.InventoryItem.ItemInstance.DurabilityPoint += Convert.ToInt32(durabilityToRepair);
        e.Sender.SendInfo(_languageService.GetLanguageFormat(GameDialogKey.MINIGAME_INFO_REFILL, e.Sender.UserLanguage, durabilityToRepair));
        e.Sender.SendMinilandDurabilityInfo(e.MapObject, _minigameConfiguration);
    }
}