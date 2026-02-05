using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.DAL.Redis.Locks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Families;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.Enums;
using WingsEmu.DTOs.Bonus;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Configurations.RaidExtraRewards;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families.Enum;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Raids;

public class RaidGiveRewardsEventHandler : IAsyncEventProcessor<RaidGiveRewardsEvent>
{
    private readonly IExpirableLockService _expirableLockService;
    private readonly IGameItemInstanceFactory _gameItemInstance;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IRaidModeConfiguration _raidModeConfiguration;
    private readonly IEvtbConfiguration _evtbConfiguration;
    private readonly RaidExtraRewardsConfiguration _raidExtraRewardsConfiguration;

    public RaidGiveRewardsEventHandler(IGameItemInstanceFactory gameItemInstance, IRandomGenerator randomGenerator, IExpirableLockService expirableLockService, 
        IEvtbConfiguration evtbConfiguration, IRaidModeConfiguration raidModeConfiguration, RaidExtraRewardsConfiguration raidExtraRewardsConfiguration)
    {
        _gameItemInstance = gameItemInstance;
        _randomGenerator = randomGenerator;
        _expirableLockService = expirableLockService;
        _raidModeConfiguration = raidModeConfiguration;
        _evtbConfiguration = evtbConfiguration;
        _raidExtraRewardsConfiguration = raidExtraRewardsConfiguration;
    }

    public async Task HandleAsync(RaidGiveRewardsEvent e, CancellationToken cancellation)
    {
        RaidParty raidParty = e.RaidParty;
        IMonsterEntity bossMap = e.MapBoss;
        RaidReward raidReward = e.RaidReward;
        RaidModeType raidMode = _raidModeConfiguration.GetModeType(raidParty.Type, raidParty.ModeType);
        
        if (bossMap == null)
        {
            return;
        }
        
        int reputation = 0;
        if (raidReward.DefaultReputation)
        {
            reputation = raidParty.MinimumLevel * 30;
        }
        else
        {
            if (raidReward.FixedReputation.HasValue)
            {
                reputation = raidReward.FixedReputation.Value;
            }
        }
        
        if (raidMode.RewardsMultiplier.Reputation > 0)
        {
            reputation *= raidMode.RewardsMultiplier.Reputation;
        }
        
        var randomBag = new RandomBag<RaidBoxRarity>(_randomGenerator);
        foreach (RaidBoxRarity toAdd in raidReward.RaidBox.RaidBoxRarities)
        {
            randomBag.AddEntry(toAdd, toAdd.Chance);
        }

        foreach (IClientSession member in raidParty.Members.ToList())
        {
            if (member == null)
            {
                continue;
            }

            if (member.CurrentMapInstance?.Id != bossMap.MapInstance?.Id)
            {
                continue;
            }

            RaidBoxRarity box = randomBag.GetRandom();
            byte boxRarity = box.Rarity;
            int randomNumber = _randomGenerator.RandomNumber(raidMode.RewardsMultiplier.Rarity, 100);
            
            int eventIncrease = _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_CHANCE_GET_SECOND_RAIDBOX);
            
            int chanceAfterEvent = randomNumber + eventIncrease; 
            
            RaidExtraReward raidExtraRewardConfig = _raidExtraRewardsConfiguration.RaidExtraRewards.FirstOrDefault(x => x.RaidType == raidParty.Type);
            
            bool boxGiven = false;
            
            if (raidExtraRewardConfig != null)
            {
                if (raidExtraRewardConfig.RewardLegendary != null && randomNumber <= raidExtraRewardConfig.RewardLegendary.Chance)
                {
                    await GiveReward(member, raidExtraRewardConfig.RewardLegendary, chanceAfterEvent);
                    boxGiven = true;
                }
                
                if (raidExtraRewardConfig.RewardPhenomenal != null && randomNumber <= raidExtraRewardConfig.RewardPhenomenal.Chance)
                {
                    await GiveReward(member, raidExtraRewardConfig.RewardPhenomenal, chanceAfterEvent);
                    boxGiven = true;
                }
                
                if (raidExtraRewardConfig.ExtraRewards != null && raidExtraRewardConfig.ExtraRewards.Count != 0)
                {
                    await GiveExtraRewards(member, raidExtraRewardConfig.ExtraRewards, chanceAfterEvent);
                }
            }
            
            if (!boxGiven)
            {
                await GiveDefaultReward(member, raidReward.RaidBox.RewardBox, chanceAfterEvent);
            }

            await member.EmitEventAsync(new RaidRewardReceivedEvent
            {
                BoxRarity = boxRarity
            });

            await ProcessFamilyExperience(member, raidParty.Type, raidParty.ModeType);

            await member.EmitEventAsync(new GenerateReputationEvent
            {
                Amount = reputation,
                SendMessage = true
            });
        }
    }
    
    private async Task GiveReward(IClientSession member, Reward reward, int chanceAfterEvent)
    {
        GameItemInstance box = _gameItemInstance.CreateItem(reward.ItemVnum, reward.Amount, reward.Upgrade, reward.Rarity);
        await member.AddNewItemToInventory(box, false, ChatMessageColorType.Yellow, true);
        member.SendSayi(ChatMessageColorType.Yellow, Game18NConstString.ReceivedItem, 2, reward.ItemVnum.ToString());

        if (member.PlayerEntity.HaveStaticBonus(StaticBonusType.JoyBellMedal))
        {
            chanceAfterEvent += 3;
        }
        
        if (member.PlayerEntity.BCardComponent.HasBCard(BCardType.PyrosphereTokenEffects, (byte)AdditionalTypes.PyrosphereTokenEffects.AdditionalRaidBoxChance))
        {
            int bonusChance = member.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.PyrosphereTokenEffects, 
                (byte)AdditionalTypes.PyrosphereTokenEffects.AdditionalRaidBoxChance, member.PlayerEntity.Level).firstData;

            if (_randomGenerator.RandomNumber() < bonusChance)
            {
                member.SendEffect(EffectType.DoubleChanceDrop);
                GameItemInstance extraBox = _gameItemInstance.CreateItem(reward.ItemVnum, reward.Amount, reward.Upgrade, reward.Rarity);
                await member.AddNewItemToInventory(extraBox, false, ChatMessageColorType.Yellow, true);
                member.SendSayi(ChatMessageColorType.Red, Game18NConstString.ExtraRaidBox);
            }
        }
        
        if (_randomGenerator.RandomNumber() < chanceAfterEvent)
        {
            member.SendEffect(EffectType.DoubleChanceDrop);
            GameItemInstance extraBox = _gameItemInstance.CreateItem(reward.ItemVnum, reward.Amount, reward.Upgrade, reward.Rarity);
            await member.AddNewItemToInventory(extraBox, false, ChatMessageColorType.Yellow, true);
            member.SendSayi(ChatMessageColorType.Red, Game18NConstString.ExtraRaidBox);
        }
    }

    private async Task GiveDefaultReward(IClientSession member, int rewardBoxVnum, int chanceAfterEvent)
    {
        GameItemInstance rewardBox = _gameItemInstance.CreateItem(rewardBoxVnum, 1);
        await member.AddNewItemToInventory(rewardBox, false, ChatMessageColorType.Yellow, true);
        member.SendSayi(ChatMessageColorType.Yellow, Game18NConstString.ReceivedItem, 2, rewardBoxVnum.ToString());

        if (member.PlayerEntity.HaveStaticBonus(StaticBonusType.JoyBellMedal))
        {
            chanceAfterEvent += 3;
        }
        
        if (member.PlayerEntity.BCardComponent.HasBCard(BCardType.PyrosphereTokenEffects, (byte)AdditionalTypes.PyrosphereTokenEffects.AdditionalRaidBoxChance))
        {
            int bonusChance = member.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.PyrosphereTokenEffects, (byte)AdditionalTypes.PyrosphereTokenEffects.AdditionalRaidBoxChance, member.PlayerEntity.Level).firstData;
            
            if (_randomGenerator.RandomNumber() < bonusChance)
            {
                member.SendEffect(EffectType.DoubleChanceDrop);
                GameItemInstance extraBox = _gameItemInstance.CreateItem(rewardBoxVnum, 1);
                await member.AddNewItemToInventory(extraBox, false, ChatMessageColorType.Yellow, true);
                member.SendSayi(ChatMessageColorType.Red, Game18NConstString.ExtraRaidBox);
            }
        }
        
        if (_randomGenerator.RandomNumber() < chanceAfterEvent)
        {
            member.SendEffect(EffectType.DoubleChanceDrop);
            GameItemInstance rewardBox2 = _gameItemInstance.CreateItem(rewardBoxVnum, 1);
            await member.AddNewItemToInventory(rewardBox2, false, ChatMessageColorType.Yellow, true);
            member.SendSayi(ChatMessageColorType.Red, Game18NConstString.ExtraRaidBox);
        }
    }
    
    private async Task GiveExtraRewards(IClientSession member, List<ExtraReward> extraRewards, int chanceAfterEvent)
    {
        foreach (ExtraReward extraReward in extraRewards)
        {
            if (_randomGenerator.RandomNumber() > extraReward.Chance)
            {
                continue;
            }
            
            GameItemInstance reward = _gameItemInstance.CreateItem(extraReward.ItemVnum, extraReward.Amount);
            await member.AddNewItemToInventory(reward, true, ChatMessageColorType.Yellow, true);
            
            if (_randomGenerator.RandomNumber() >= chanceAfterEvent)
            {
                continue;
            }
            
            member.SendEffect(EffectType.DoubleChanceDrop);
            GameItemInstance rewardExtra = _gameItemInstance.CreateItem(extraReward.ItemVnum, extraReward.Amount);
            await member.AddNewItemToInventory(rewardExtra, false, ChatMessageColorType.Yellow, true);
        }
    }

    private async Task ProcessFamilyExperience(IClientSession member, RaidType raidPartyType, ModeType raidModeType)
    {
        RaidModeType raidMode = _raidModeConfiguration.GetModeType(raidPartyType, raidModeType);
        
        if (!member.PlayerEntity.IsInFamily())
        {
            return;
        }

        if (!await _expirableLockService.TryAddTemporaryLockAsync(
                $"game:locks:family:{member.PlayerEntity.Id}:raids:{(short)raidPartyType}:character:{member.PlayerEntity.Id}", DateTime.UtcNow.Date.AddDays(1)))
        {
            return;
        }

        int experience = 200;
        
        experience *= raidMode.RewardsMultiplier.Fxp;
        
        await member.EmitEventAsync(new FamilyAddExperienceEvent(experience, FamXpObtainedFromType.Raid));
    }
}