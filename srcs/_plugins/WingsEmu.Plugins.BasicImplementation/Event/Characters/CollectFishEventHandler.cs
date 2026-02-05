using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums;
using System;
using WingsEmu.Game;
using PhoenixLib.Scheduler;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Managers.StaticData;
using System.Collections.Generic;
using System.Linq;
using WingsAPI.Data.Character;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Items;
using WingsEmu.Packets.Enums.Chat;
using WingsAPI.Data.Fish;
using WingsAPI.Packets.Enums;
using WingsAPI.Packets.Enums.BattlePass;
using WingsEmu.Game._enum;
using WingsEmu.Game.Fish;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters
{
    internal class CollectFishEventHandler : IAsyncEventProcessor<CollectFishEvent>
    {
        private readonly IScheduler _scheduler;
        private readonly IRandomGenerator _randomGenerator;
        private readonly FishConfiguration _fishConfiguration;
        private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
        private readonly IEvtbConfiguration _evtbConfiguration;
        private readonly IItemsManager _itemsManager;
        private readonly IFishManager _fishManager;
        public CollectFishEventHandler(IScheduler scheduler, IRandomGenerator randomGenerator, FishConfiguration fishConfiguration, IGameItemInstanceFactory gameItemInstanceFactory,
            IEvtbConfiguration evtbConfiguration, IItemsManager itemsManager, IFishManager fishManager)
        {
            _scheduler = scheduler;
            _randomGenerator = randomGenerator;
            _fishConfiguration = fishConfiguration;
            _gameItemInstanceFactory = gameItemInstanceFactory;
            _evtbConfiguration = evtbConfiguration;
            _itemsManager = itemsManager;
            _fishManager = fishManager;
        }

        public static bool ContainsAllItems(List<short> a, List<short> b) => a.All(b.Contains);

        public async Task HandleAsync(CollectFishEvent e, CancellationToken cancellation)
        {
            IPlayerEntity player = e.Sender.PlayerEntity;
            
            _scheduler.Schedule(TimeSpan.FromMilliseconds(2000), async () =>
            {
                await player.RemoveBuffAsync((int)BuffVnums.FISH_LINE);
            });
            
            switch (player)
            {
                case { CanCollectFish: false }:
                    player.Session.SendSayi(ChatMessageColorType.White, Game18NConstString.BadLuck);
                    player.Session.CurrentMapInstance.Broadcast(player.Session.GenerateGuriPacket(6, 1, player.Id, 44));
                    return;
                case null:
                    return;
            }

            int random = _randomGenerator.RandomNumber(0, 100);

            // Base probabilities: [Line Breaks, Catch Nothing, Catch Something]
            byte[] baseRate = [25, 25, 50]; // Adjust these as needed

            // Apply player's bonuses or penalties
            double moreBreakChance = player.BCardComponent.GetAllBCardsInformation(BCardType.Fishing, (byte)AdditionalTypes.Fishing.MoreChanceToBreak, player.Level).firstData * 0.01;
            double lessBreakChance = player.BCardComponent.GetAllBCardsInformation(BCardType.Fishing, (byte)AdditionalTypes.Fishing.LessChanceToBreak, player.Level).firstData * 0.01;

            double moreCatchChance = player.BCardComponent.GetAllBCardsInformation(BCardType.Fishing, (byte)AdditionalTypes.Fishing.MoreChanceToCatch, player.Level).firstData * 0.01;
            double lessCatchChance = player.BCardComponent.GetAllBCardsInformation(BCardType.Fishing, (byte)AdditionalTypes.Fishing.LessChanceToCatch, player.Level).firstData * 0.01;

            // Adjusting break chance based on player's BCard information
            double adjustedBreakChance = baseRate[0] + (moreBreakChance - lessBreakChance) * 100;
            adjustedBreakChance = Math.Max(0, Math.Min(adjustedBreakChance, 100)); // Ensure within bounds

            // Adjusting catch chance based on player's BCard information, ensuring total probabilities sum to 100
            double adjustedCatchNothingChance = baseRate[1] - (moreCatchChance - lessCatchChance) * 100;
            adjustedCatchNothingChance = Math.Max(0, Math.Min(adjustedCatchNothingChance, 100 - adjustedBreakChance));
            
            // Determine the outcome based on the random number and adjusted probabilities
            if (random < adjustedBreakChance)
            {
                await player.Session.EmitEventAsync(new InventoryRemoveItemEvent((int)ItemVnums.FISHING_LINE));
                _scheduler.Schedule(TimeSpan.FromMilliseconds(2500), () =>
                {
                    player.Session.SendSayi(ChatMessageColorType.White, Game18NConstString.FishingLineBroke);
                    return Task.CompletedTask;
                });
                player.Session.CurrentMapInstance.Broadcast(player.Session.GenerateGuriPacket(6, 1, player.Id, 45));
                player.HasFishingLineBroke = true;
            }
            else if (random < adjustedBreakChance + adjustedCatchNothingChance)
            {
                player.Session.SendSayi(ChatMessageColorType.White, Game18NConstString.BadLuck);
                player.Session.CurrentMapInstance.Broadcast(player.Session.GenerateGuriPacket(6, 1, player.Id, 44));
                player.HasBadLuck = true;
            }
            else
            {
                RewardFishType rewardsType = RewardFishType.NormalFish;
                player.HasCaughtFish = true;

                if (_randomGenerator.RandomNumber() < 5)
                {
                    rewardsType = RewardFishType.Items;
                }

                if (player.IsRareFish)
                {
                    rewardsType = RewardFishType.RareFish;
                }
                
                if (_randomGenerator.RandomNumber() < 10)
                {
                    player.Session.CurrentMapInstance.Broadcast(player.Session.GenerateGuriPacket(6, 1, player.Id, 45));
                    player.Session.SendSayi(ChatMessageColorType.White, Game18NConstString.FishAitBait);
                    player.HasCaughtFish = false;
                    return;
                }

                FishingSpotDto spot = _fishManager.GetFishSpotByMapId(player.MapInstance.MapId);
                FishingRewardsDto rewards = _fishManager.GetRewardsBySpot(spot, rewardsType);
                short amountFish = 1;
                double lengthFish = 0;

                player.Session.CurrentMapInstance.Broadcast(player.Session.GenerateGuriPacket(6, 1, player.Id, 42));

                _scheduler.Schedule(TimeSpan.FromMilliseconds(2000), () => 
                {
                    short item = rewards.RewardsVnum;
                    player.Session.SendSayi(ChatMessageColorType.Red, Game18NConstString.ReceivedThisItem, 2, item, amountFish);
                    if (rewardsType != RewardFishType.Items)
                    {
                        FishInfo info = _fishConfiguration.FishInfo[item];

                        lengthFish = Math.Abs(_randomGenerator.RandomNumber(info.MinSize, info.MaxSize));
                        CharacterFishDto alreadyHaveTheFish = player.FishDto.FirstOrDefault(s => s.FishVnum == item);

                        (int firstData, int secondData) = player.BCardComponent.GetAllBCardsInformation(BCardType.Fishing, (byte)AdditionalTypes.Fishing.IncreaseFishSize, player.Level);

                        if (firstData != 0 && _randomGenerator.RandomNumber() < firstData)
                        {
                            lengthFish = lengthFish * secondData / 100;
                        }

                        if (lengthFish > info.MaxSize)
                        {
                            lengthFish = info.MaxSize;
                        }

                        int percent = player.BCardComponent.GetAllBCardsInformation(BCardType.IncreaseElementFairy, (byte)AdditionalTypes.IncreaseElementFairy.ProvideExtraFish, player.Level).firstData;
                        if (percent != 0 && _randomGenerator.RandomNumber() < percent)
                        {
                            amountFish = 2;
                        }

                        if (alreadyHaveTheFish == null)
                        {
                            var rewardsBySpot = new Dictionary<byte, List<short>>
                            {
                                [0] = [(int)ItemVnums.TITLE_POND_LIFE, 2282],
                                [1] = [(int)ItemVnums.TITLE_THE_PRICE_OF_FISH],
                                [2] = [(int)ItemVnums.TITLE_BASS_KICKER],
                                [3] = [(int)ItemVnums.TITLE_BLUE_WATER_FISHER],
                                [4] = [(int)ItemVnums.TITLE_JAWS],
                                [5] = [(int)ItemVnums.TITLE_FISH_OUT_OF_WATER],
                                [6] = [(int)ItemVnums.TITLE_OCTO_CATCHER],
                                [7] = [(int)ItemVnums.TITLE_ALLIGATOR],
                                [8] = [(int)ItemVnums.TITLE_SOMETHING_FISHY],
                                [9] = [(int)ItemVnums.TITLE_MERMAID],
                                [10] = [(int)ItemVnums.TITLE_BIG_FISH_TO_FRY],
                            };

                            alreadyHaveTheFish = new CharacterFishDto
                            {
                                Amount = amountFish,
                                FishVnum = item,
                                MaxLenght = lengthFish,
                            };
                            
                            player.FishDto.Add(alreadyHaveTheFish);
                            player.Session.SendSayi(ChatMessageColorType.Green, Game18NConstString.CaughtFishFirstTime, 2, item, amountFish);
                            
                            int minLvl = spot.MinLvl;
                            int maxLvl = spot.MaxLvl;
                            
                            var allFishRewardsForCurrentSpot = _fishManager.GetFishSpotByMapId(player.MapInstance.MapId).Rewards.Where(s => !s.IsMaterial).Select(s => s.RewardsVnum).ToList();
                            var currentFishObtained = player.FishDto.Select(s => (short)s.FishVnum).ToList();

                            if (ContainsAllItems(allFishRewardsForCurrentSpot, currentFishObtained))
                            {
                                player.Session.SendSayi(ChatMessageColorType.Green, Game18NConstString.CaughtAllFish, 2, minLvl, maxLvl);
                            }
                            
                            foreach (FishingSpotDto fish in _fishManager.GetAllFishSpotByIndex())
                            {
                                if (!rewardsBySpot.TryGetValue((byte)fish.FishVnum, out List<short> rewardItemsVNums))
                                {
                                    continue;
                                }

                                foreach (short rewardItemVNum in rewardItemsVNums)
                                {
                                    if (player.FishRewardsEarnedDto.Any(s => s.Vnum == rewardItemVNum))
                                    {
                                        continue;
                                    }

                                    var allFishRewards = fish.Rewards.Where(s => !s.IsMaterial).Select(s => s.RewardsVnum).ToList();
                                    var currentFish = player.FishDto.Select(s => (short)s.FishVnum).ToList();
                                    
                                    if (!ContainsAllItems(allFishRewards, currentFish))
                                    {
                                        continue;
                                    }
                                    
                                    GameItemInstance rewardItem = _gameItemInstanceFactory.CreateItem(rewardItemVNum);
                                    short slotTitle = player.GetNextInventorySlot(rewardItem.GameItem.Type);
                                    InventoryType typeTitle = rewardItem.GameItem.Type;
                                    var inventoryItemTitle = new InventoryItem
                                    {
                                        InventoryType = typeTitle,
                                        IsEquipped = false,
                                        ItemInstance = rewardItem,
                                        CharacterId = player.Id,
                                        Slot = slotTitle
                                    };
                                    
                                    player.Session.EmitEvent(new InventoryAddItemEvent(inventoryItemTitle, false, ChatMessageColorType.Green, true, MessageErrorType.Chat, slotTitle, typeTitle));
                                    player.FishRewardsEarnedDto.Add(new CharacterFishRewardsEarnedDto
                                    {
                                        Vnum = rewardItemVNum
                                    });
                                }
                            }


                            if (ContainsAllItems(_fishManager.GetAllRewardsFromEachSpotByIndex().Select(s => s.RewardsVnum).ToList(), player.FishDto.Select(s => (short)s.FishVnum).ToList()))
                            {
                                bool hasRewarded = false;

                                var itemVNumsToCheck = new List<int>
                                {
                                    (int)ItemVnums.TITLE_CHAMPION_ANGLER,
                                    (int)ItemVnums.BOOK_THE_ANGLERS_BIBLE
                                };

                                foreach (int itemVNum in itemVNumsToCheck)
                                {
                                    if (player.FishRewardsEarnedDto.Any(s => s.Vnum == itemVNum))
                                    {
                                        continue;
                                    }

                                    GameItemInstance gameItemInstance = _gameItemInstanceFactory.CreateItem(itemVNum);
                                    short inventorySlot = player.GetNextInventorySlot(gameItemInstance.GameItem.Type);
                                    InventoryType inventoryType = gameItemInstance.GameItem.Type;

                                    var newInventoryItem = new InventoryItem
                                    {
                                        InventoryType = inventoryType,
                                        IsEquipped = false,
                                        ItemInstance = gameItemInstance,
                                        CharacterId = player.Id,
                                        Slot = inventorySlot
                                    };

                                    player.Session.EmitEvent(new InventoryAddItemEvent(newInventoryItem, false, ChatMessageColorType.Green, true, MessageErrorType.Chat, inventorySlot, inventoryType));
                                    player.FishRewardsEarnedDto.Add(new CharacterFishRewardsEarnedDto { Vnum = itemVNum });
                                    hasRewarded = true;
                                }
                                
                                if (hasRewarded)
                                {
                                    player.Session.SendSayi(ChatMessageColorType.Green, Game18NConstString.EncyclopediaOfFishComplete);
                                }
                            }
                        }
                        else
                        {
                            alreadyHaveTheFish.Amount++;
                            if (alreadyHaveTheFish.MaxLenght < lengthFish)
                            {
                                player.Session.SendSayi(ChatMessageColorType.Green, Game18NConstString.CaughtBiggerFish, 2, item);
                                alreadyHaveTheFish.MaxLenght = lengthFish;
                            }
                        }
                        player.Session.SendPacket(player.Session.GenerateFish2Packet(_itemsManager, item, Convert.ToInt32(lengthFish), alreadyHaveTheFish.Amount));

                        double moreExp = 1 + player.BCardComponent.GetAllBCardsInformation(BCardType.Fishing, (byte)AdditionalTypes.Fishing.FishXpIncreasedBy, player.Level).firstData * 0.01;
                        moreExp -= player.BCardComponent.GetAllBCardsInformation(BCardType.Fishing, (byte)AdditionalTypes.Fishing.FishXpDecreasedBy, player.Level).firstData * 0.01;
                        moreExp += _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_FISHING_EXPERIENCE_GAIN) * 0.01;

                        player.Session.EmitEvent(new AddExpEvent((long)(info.ExpCollected * moreExp), LevelType.SpJobLevel));
                        player.Session.EmitEvent(new IncreaseBattlePassObjectiveEvent(MissionType.CatchXFish));
                        
                        GameItemInstance itemInstance = _gameItemInstanceFactory.CreateItem(item, amountFish);
                        short slot = player.GetNextInventorySlot(itemInstance.GameItem.Type);
                        InventoryType type = itemInstance.GameItem.Type;
                        var inventoryItem = new InventoryItem
                        {
                            InventoryType = type,
                            IsEquipped = false,
                            ItemInstance = itemInstance,
                            CharacterId = player.Id,
                            Slot = slot
                        };
                        
                        player.Session.EmitEvent(new InventoryAddItemEvent(inventoryItem, false, ChatMessageColorType.Green, true, MessageErrorType.Chat, slot, type));
                    }

                    player.Session.SendPacket(rewardsType == RewardFishType.Items ? $"pdti {(int)PdtiType.ItemIsObtained} {item} -1 -1 -1 -1" : $"pdti {(int)PdtiType.Fishing} {item} {amountFish} {Convert.ToInt32(lengthFish)} -1 -1");
                });
            }
        }
    }
}