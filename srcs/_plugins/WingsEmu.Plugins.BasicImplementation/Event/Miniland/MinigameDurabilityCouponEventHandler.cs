using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.MinilandExtensions;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations.Miniland;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Miniland.Events;

namespace NosEmu.Plugins.BasicImplementations.Event.Miniland;

public class MinigameDurabilityCouponEventHandler : IAsyncEventProcessor<MinigameDurabilityCouponEvent>
{
    private const MinigameInteraction ThisAction = MinigameInteraction.UseDurabilityCoupon;
    private readonly IGameLanguageService _languageService;
    private readonly MinigameConfiguration _minigameConfiguration;
    private readonly IMinigameManager _minigameManager;

    public MinigameDurabilityCouponEventHandler(MinigameConfiguration minigameConfiguration, IGameLanguageService languageService, IMinigameManager minigameManager)
    {
        _minigameConfiguration = minigameConfiguration;
        _languageService = languageService;
        _minigameManager = minigameManager;
    }

    public async Task HandleAsync(MinigameDurabilityCouponEvent e, CancellationToken cancellation)
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
            await e.Sender.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "Tried to repair with a coupon a minigame that he doesn't own." +
                $" 'SuspectCharacterId': {e.Sender.PlayerEntity.Id.ToString()} | 'VictimCharacterId': {e.MapObject.CharacterId.ToString()}");
            return;
        }

        if (e.MapObject.InventoryItem.ItemInstance.DurabilityPoint >= e.MapObject.InventoryItem.ItemInstance.GameItem.MinilandObjectPoint)
        {
            e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.MINILAND_INFO_DURABILITY_MAXIMUM, e.Sender.UserLanguage));
            return;
        }

        if (!e.Sender.PlayerEntity.HasItem(_minigameConfiguration.Configuration.RepairDurabilityCouponVnum))
        {
            await e.Sender.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "Tried to 'coupon repair' without a coupon" +
                $" 'SuspectCharacterId': {e.Sender.PlayerEntity.Id.ToString()} | 'VictimCharacterId': {e.MapObject.CharacterId.ToString()}");
            return;
        }

        await e.Sender.RemoveItemFromInventory(_minigameConfiguration.Configuration.RepairDurabilityCouponVnum);
        _minigameManager.RegisterInteraction(e.Sender, new MinilandInteractionInformationHolder(ThisAction, e.MapObject));

        int pointsToAdd = _minigameConfiguration.Configuration.DurabilityCouponRepairingAmount;

        if (e.MapObject.InventoryItem.ItemInstance.DurabilityPoint + pointsToAdd > e.MapObject.InventoryItem.ItemInstance.GameItem.MinilandObjectPoint)
        {
            pointsToAdd = e.MapObject.InventoryItem.ItemInstance.GameItem.MinilandObjectPoint - e.MapObject.InventoryItem.ItemInstance.DurabilityPoint;
            e.MapObject.InventoryItem.ItemInstance.DurabilityPoint = e.MapObject.InventoryItem.ItemInstance.GameItem.MinilandObjectPoint;
        }
        else
        {
            e.MapObject.InventoryItem.ItemInstance.DurabilityPoint += pointsToAdd;
        }

        e.Sender.SendInfo(_languageService.GetLanguageFormat(GameDialogKey.MINIGAME_INFO_REFILL, e.Sender.UserLanguage, pointsToAdd));
        e.Sender.SendMinilandDurabilityInfo(e.MapObject, _minigameConfiguration);
    }
}