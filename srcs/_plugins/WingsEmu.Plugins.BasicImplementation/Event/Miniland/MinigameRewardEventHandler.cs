using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.MinilandExtensions;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations.Miniland;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Miniland.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Event.Miniland;

public class MinigameRewardEventHandler : IAsyncEventProcessor<MinigameRewardEvent>
{
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IGameLanguageService _languageService;
    private readonly MinigameConfiguration _minigameConfiguration;
    private readonly IMinigameManager _minigameManager;
    private readonly IRandomGenerator _randomGenerator;
    private readonly MinigameInteraction ThisAction = MinigameInteraction.GetReward;

    public MinigameRewardEventHandler(IMinigameManager minigameManager, IRandomGenerator randomGenerator, MinigameConfiguration minigameConfiguration, IGameLanguageService languageService,
        IGameItemInstanceFactory gameItemInstanceFactory)
    {
        _minigameManager = minigameManager;
        _randomGenerator = randomGenerator;
        _minigameConfiguration = minigameConfiguration;
        _languageService = languageService;
        _gameItemInstanceFactory = gameItemInstanceFactory;
    }

    public async Task HandleAsync(MinigameRewardEvent e, CancellationToken cancellation)
    {
        MinilandInteractionInformationHolder possibleOldScore = _minigameManager.GetLastInteraction(e.Sender);
        if (possibleOldScore.Interaction != MinigameInteraction.DeclarateScore
            && possibleOldScore.MapObject != e.MapObject)
        {
            _minigameManager.ReportInteractionIncoherence(e.Sender, possibleOldScore.Interaction, possibleOldScore.MapObject, ThisAction, e.MapObject);
            return;
        }

        if (possibleOldScore.MapObject != e.MapObject || possibleOldScore.SavedRewards == default)
        {
            return;
        }

        if ((int)e.RewardLevel > (int)possibleOldScore.SavedRewards.maxRewardLevel)
        {
            await e.Sender.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "Incoherence in the rewards offered and the reward reclaimed.");
            e.Sender.SendMinigameRewardLevel(possibleOldScore.SavedRewards.maxRewardLevel);
            return;
        }

        MinigameRewards rewardsToGive = possibleOldScore.SavedRewards.rewards.FirstOrDefault(x => x.RewardLevel == e.RewardLevel);
        if (rewardsToGive == default || rewardsToGive.Rewards.Count < 1)
        {
            e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.MINIGAME_INFO_NO_REWARD, e.Sender.UserLanguage));
            e.Sender.SendMinigameReward(0, 0);
            return;
        }

        if (e.Coupon && !e.Sender.PlayerEntity.HasItem(_minigameConfiguration.Configuration.DoubleRewardCouponVnum))
        {
            e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.MINILAND_INFO_NO_ENOUGH_REWARD_COUPON, e.Sender.UserLanguage));
            await e.Sender.NotifyStrangeBehavior(StrangeBehaviorSeverity.NORMAL, "Tried to do coupon reward without coupon.");
            return;
        }

        if (e.MapObject.InventoryItem.ItemInstance.DurabilityPoint < rewardsToGive.DurabilityCost)
        {
            e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.MINILAND_INFO_NOT_ENOUGH_DURABILITY_POINT, e.Sender.UserLanguage));
            return;
        }

        if (e.Sender.PlayerEntity.MinilandPoint < _minigameConfiguration.Configuration.MinigamePointsCostPerMinigame)
        {
            e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.MINILAND_INFO_NOT_ENOUGH_PRODUCTION_POINTS, e.Sender.UserLanguage));
            return;
        }

        e.MapObject.InventoryItem.ItemInstance.DurabilityPoint -= rewardsToGive.DurabilityCost;

        int costReduction = e.Sender.PlayerEntity.GetMaxArmorShellValue(ShellEffectType.ReducedProductionPointConsumed);
        costReduction += (costReduction * e.Sender.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.IncreaseDamageVersusMonsters_2, (byte)AdditionalTypes.IncreaseDamageVersusMonsters_2.ProductionPointsUseDecrease, e.Sender.PlayerEntity.Level).firstData / 100);
        costReduction -= (costReduction * e.Sender.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.IncreaseDamageVersusMonsters_2, (byte)AdditionalTypes.IncreaseDamageVersusMonsters_2.ProductionPointsUseDecreaseNegated, e.Sender.PlayerEntity.Level).firstData / 100);

        int toRemove = _minigameConfiguration.Configuration.MinigamePointsCostPerMinigame - costReduction;

        if (toRemove < 0)
        {
            toRemove = 0;
        }
        e.Sender.RemoveMinigamePoints((short)toRemove, _minigameConfiguration);

        _minigameManager.RegisterInteraction(e.Sender, new MinilandInteractionInformationHolder(ThisAction, e.MapObject));

        MinigameReward rewardToGive = rewardsToGive.Rewards[_randomGenerator.RandomNumber(0, rewardsToGive.Rewards.Count)];
        short itemAmount = (short)Math.Round(rewardToGive.Amount * (1 + e.Sender.PlayerEntity.GetMaxArmorShellValue(ShellEffectType.IncreasedProductionPossibility) * 0.01));
        if (e.Coupon)
        {
            await e.Sender.RemoveItemFromInventory(_minigameConfiguration.Configuration.DoubleRewardCouponVnum);
            //This is because amount > 1 in things like weapons doesn't work.
            await GiveReward(e.Sender, rewardToGive.Vnum, itemAmount);
        }

        await GiveReward(e.Sender, rewardToGive.Vnum, itemAmount);
        e.Sender.SendMinigameReward(rewardToGive.Vnum, e.Coupon ? itemAmount * 2 : itemAmount);

        Minigame minigameConfiguration = _minigameManager.GetSpecificMinigameConfiguration(e.MapObject.InventoryItem.ItemInstance.ItemVNum);
        await e.Sender.EmitEventAsync(new MinigameRewardClaimedEvent
        {
            OwnerId = e.MapObject.CharacterId,
            MinigameVnum = e.MapObject.InventoryItem.ItemInstance.ItemVNum,
            MinigameType = minigameConfiguration.Type,
            RewardLevel = rewardsToGive.RewardLevel,
            Coupon = e.Coupon,
            ItemVnum = rewardToGive.Vnum,
            Amount = e.Coupon ? (short)(rewardToGive.Amount * 2) : (short)rewardToGive.Amount
        });

        if (e.MapObject.CharacterId == e.Sender.PlayerEntity.Id)
        {
            return;
        }

        switch (e.RewardLevel)
        {
            case RewardLevel.FirstReward:
                e.MapObject.Level1BoxAmount++;
                break;

            case RewardLevel.SecondReward:
                e.MapObject.Level2BoxAmount++;
                break;

            case RewardLevel.ThirdReward:
                e.MapObject.Level3BoxAmount++;
                break;

            case RewardLevel.FourthReward:
                e.MapObject.Level4BoxAmount++;
                break;

            case RewardLevel.FifthReward:
                e.MapObject.Level5BoxAmount++;
                break;
        }

        //TODO Message notificating player did minigame. Should be configurable individuallly (per player)
    }

    private async Task GiveReward(IClientSession session, int vnum, int amount)
    {
        GameItemInstance item = _gameItemInstanceFactory.CreateItem(vnum, amount);
        await session.AddNewItemToInventory(item, sendGiftIsFull: true);
    }
}