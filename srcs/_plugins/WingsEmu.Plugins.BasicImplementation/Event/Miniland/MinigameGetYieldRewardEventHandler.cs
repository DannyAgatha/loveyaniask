using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.MinilandExtensions;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations.Miniland;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Miniland.Events;

namespace NosEmu.Plugins.BasicImplementations.Event.Miniland;

public class MinigameGetYieldRewardEventHandler : IAsyncEventProcessor<MinigameGetYieldRewardEvent>
{
    private const MinigameInteraction ThisAction = MinigameInteraction.GetYieldReward;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IGameLanguageService _languageService;
    private readonly MinigameConfiguration _minigameConfiguration;
    private readonly IMinigameManager _minigameManager;
    private readonly IRandomGenerator _randomGenerator;

    public MinigameGetYieldRewardEventHandler(MinigameConfiguration minigameConfiguration, IGameLanguageService languageService, IMinigameManager minigameManager, IRandomGenerator randomGenerator,
        IGameItemInstanceFactory gameItemInstanceFactory)
    {
        _minigameConfiguration = minigameConfiguration;
        _languageService = languageService;
        _minigameManager = minigameManager;
        _randomGenerator = randomGenerator;
        _gameItemInstanceFactory = gameItemInstanceFactory;
    }

    public async Task HandleAsync(MinigameGetYieldRewardEvent e, CancellationToken cancellation)
    {
        MinilandInteractionInformationHolder minilandInteraction = _minigameManager.GetLastInteraction(e.Sender);

        if (minilandInteraction.Interaction != MinigameInteraction.GetYieldInformation
            && minilandInteraction.Interaction != ThisAction
            && minilandInteraction.MapObject != e.MapObject)
        {
            _minigameManager.ReportInteractionIncoherence(e.Sender, minilandInteraction.Interaction, minilandInteraction.MapObject, ThisAction, e.MapObject);
            return;
        }

        if (e.MapObject.CharacterId != e.Sender.PlayerEntity.Id)
        {
            await e.Sender.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "Tried to steal the yield rewards from a minigame that he doesn't own." +
                $" 'SuspectCharacterId': {e.Sender.PlayerEntity.Id.ToString()} | 'VictimCharacterId': {e.MapObject.CharacterId.ToString()}");
            return;
        }

        MinigameRewards rewardsToGive =
            _minigameManager.GetSpecificMinigameConfiguration(e.MapObject.InventoryItem.ItemInstance.ItemVNum).Rewards.FirstOrDefault(x => x.RewardLevel == e.RewardLevel);

        if (rewardsToGive == default || rewardsToGive.Rewards.Count < 1)
        {
            e.Sender.SendInfo(_languageService.GetLanguage(GameDialogKey.MINIGAME_INFO_NO_REWARD, e.Sender.UserLanguage));
            return;
        }

        int amount = GetAmount(e);
        if (amount < 1)
        {
            await e.Sender.NotifyStrangeBehavior(StrangeBehaviorSeverity.NORMAL, $"The quantity of the minigame requested in GetYieldReward is 0 -> 'RewardLevel': {e.RewardLevel}");
            return;
        }

        _minigameManager.RegisterInteraction(e.Sender, new MinilandInteractionInformationHolder(ThisAction, e.MapObject));
        var dictionary = new Dictionary<int, int>();

        for (int i = 0; i < amount; i++)
        {
            MinigameReward reward = rewardsToGive.Rewards[_randomGenerator.RandomNumber(0, rewardsToGive.Rewards.Count)];
            if (dictionary.ContainsKey(reward.Vnum))
            {
                dictionary[reward.Vnum] += reward.Amount;
                continue;
            }

            dictionary.TryAdd(reward.Vnum, reward.Amount);
        }

        var list = e.MapObject.GetYieldRewardEnumerable().ToList();

        foreach ((int vnum, int amount1) in dictionary)
        {
            GameItemInstance item = _gameItemInstanceFactory.CreateItem(vnum, amount1);
            await e.Sender.AddNewItemToInventory(item, sendGiftIsFull: true);
            list.Add(new MinigameReward
            {
                Amount = amount1,
                Vnum = vnum
            });
        }

        e.Sender.SendMinilandYieldInfo(e.MapObject, list, _minigameConfiguration);
    }

    public ushort GetAmount(MinigameGetYieldRewardEvent e)
    {
        ushort amount;
        switch (e.RewardLevel)
        {
            case RewardLevel.FirstReward:
                amount = e.MapObject.Level1BoxAmount;
                e.MapObject.Level1BoxAmount = 0;
                break;
            case RewardLevel.SecondReward:
                amount = e.MapObject.Level2BoxAmount;
                e.MapObject.Level2BoxAmount = 0;
                break;
            case RewardLevel.ThirdReward:
                amount = e.MapObject.Level3BoxAmount;
                e.MapObject.Level3BoxAmount = 0;
                break;
            case RewardLevel.FourthReward:
                amount = e.MapObject.Level4BoxAmount;
                e.MapObject.Level4BoxAmount = 0;
                break;
            case RewardLevel.FifthReward:
                amount = e.MapObject.Level5BoxAmount;
                e.MapObject.Level5BoxAmount = 0;
                break;
            default:
                e.Sender.NotifyStrangeBehavior(StrangeBehaviorSeverity.NORMAL, $"RewardLevel requested in GetYieldReward is incoherent -> 'RewardLevel': {e.RewardLevel}");
                throw new ArgumentOutOfRangeException();
        }

        return amount;
    }
}