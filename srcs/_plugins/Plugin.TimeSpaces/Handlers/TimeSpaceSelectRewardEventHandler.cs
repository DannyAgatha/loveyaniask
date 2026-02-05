using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.Quests;
using WingsAPI.Packets.Enums.BattlePass;
using WingsEmu.DTOs.Quests;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpaceSelectRewardEventHandler : IAsyncEventProcessor<TimeSpaceSelectRewardEvent>
{
    private readonly IDropRarityConfigurationProvider _dropRarityConfigurationProvider;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IGameLanguageService _gameLanguageService;
    private readonly IItemsManager _itemsManager;
    private readonly IQuestManager _questManager;
    private readonly IRandomGenerator _randomGenerator;

    public TimeSpaceSelectRewardEventHandler(IRandomGenerator randomGenerator, IGameItemInstanceFactory gameItemInstanceFactory, IItemsManager itemsManager,
        IDropRarityConfigurationProvider dropRarityConfigurationProvider, IGameLanguageService gameLanguageService, IQuestManager questManager)
    {
        _randomGenerator = randomGenerator;
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _itemsManager = itemsManager;
        _dropRarityConfigurationProvider = dropRarityConfigurationProvider;
        _gameLanguageService = gameLanguageService;
        _questManager = questManager;
    }

    public async Task HandleAsync(TimeSpaceSelectRewardEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        if (session.CurrentMapInstance.MapInstanceType != MapInstanceType.TimeSpaceInstance)
        {
            return;
        }

        TimeSpaceParty timeSpace = session.PlayerEntity.TimeSpaceComponent.TimeSpace;
        if (timeSpace?.Instance == null)
        {
            return;
        }

        if (!timeSpace.Finished)
        {
            return;
        }

        if (timeSpace.ClaimedRewards != null && timeSpace.ClaimedRewards.Contains(session.PlayerEntity.Id))
        {
            return;
        }

        var rewardsToGive = new List<TimeSpaceRewardItem>();
        TimeSpaceRewards timeSpaceReward = timeSpace.TimeSpaceInformation?.Rewards;

        if (timeSpaceReward == null)
        {
            Log.Error($"Couldn't take information about the Time-Space rewards for TS-Id: {timeSpace.TimeSpaceId} - PlayerId: {session.PlayerEntity.Id}", new Exception());
            return;
        }

        TimeSpaceRewardItem randomDrawItem = null;

        if (timeSpaceReward.Draw != null && timeSpaceReward.Draw.Any() && !timeSpace.IsEasyMode)
        {
            int rand = _randomGenerator.RandomNumber(0, timeSpaceReward.Draw.Count);
            TimeSpaceItemConfiguration randomDraw = timeSpaceReward.Draw[rand];
            randomDrawItem = new TimeSpaceRewardItem
            {
                Type = TimeSpaceRewardType.DRAW,
                ItemVnum = randomDraw.ItemVnum,
                Amount = randomDraw.Amount
            };
            rewardsToGive.Add(randomDrawItem);
        }

        if (timeSpace.FirstCompletedTimeSpaceIds.Contains(session.PlayerEntity.Id) && timeSpaceReward.Special != null)
        {
            foreach (TimeSpaceItemConfiguration special in timeSpaceReward.Special)
            {
                rewardsToGive.Add(new TimeSpaceRewardItem
                {
                    Type = TimeSpaceRewardType.SPECIAL,
                    ItemVnum = special.ItemVnum,
                    Amount = special.Amount
                });
            }
        }

        if (timeSpaceReward.Bonus != null && !timeSpace.IsEasyMode)
        {
            foreach (TimeSpaceItemConfiguration bonus in timeSpaceReward.Bonus)
            {
                if (bonus.ItemVnum == (int)ItemVnums.SOUND_FLOWER)
                {
                    if (session.GetEmptyQuestSlot(QuestSlotType.GENERAL) == -1)
                    {
                        session.SendMsg(_gameLanguageService.GetLanguage(GameDialogKey.QUEST_SHOUTMESSAGE_SLOT_FULL, session.UserLanguage), MsgMessageType.Middle);
                    }
                    else
                    {
                        session.PlayerEntity.IncreasePendingSoundFlowerQuests();
                        session.RefreshQuestList(_questManager, null);
                    }

                    continue;
                }

                rewardsToGive.Add(new TimeSpaceRewardItem
                {
                    Type = TimeSpaceRewardType.BONUS,
                    ItemVnum = bonus.ItemVnum,
                    Amount = bonus.Amount
                });
            }
        }

        timeSpace.ClaimedRewards?.Add(session.PlayerEntity.Id);

        foreach (TimeSpaceRewardItem item in rewardsToGive)
        {
            IGameItem gameItem = _itemsManager.GetItem(item.ItemVnum);
            sbyte randomRarity = _dropRarityConfigurationProvider.GetRandomRarity(gameItem.ItemType);
            item.Rarity = randomRarity;

            GameItemInstance itemToAdd = _gameItemInstanceFactory.CreateItem(item.ItemVnum, item.Amount, 0, randomRarity);
            await session.AddNewItemToInventory(itemToAdd, sendGiftIsFull: true);
        }

        if (e.SendRepayPacket)
        {
            session.SendRepayPacket(rewardsToGive, randomDrawItem);
        }

        timeSpace.FinishTimeSpace(DateTime.UtcNow.AddMinutes(1));
        
        if (timeSpace.TimeSpaceInformation.MinLevel >= 30) 
        { 
            await session.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.CompleteXTimeSpaceLvl30Plus)); 
        }

        switch (timeSpace.TimeSpaceInformation.IsHidden)
        {
            case false:
                await session.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.CompleteXTimeSpaceExcHidden));
                break;
            case true:
                await session.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.CompleteXHiddenTimeSpace));
                break;
        }
    }
}