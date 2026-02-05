using PhoenixLib.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Rewards;
using WingsAPI.Communication.Sessions.Model;
using WingsAPI.Game.Extensions.Families;
using WingsAPI.Game.Extensions.Groups;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsEmu.DTOs.Mails;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families.Enum;
using WingsEmu.Game.Items;
using WingsEmu.Game.Mails.Events;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums.Families;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class LevelUpEventHandler : IAsyncEventProcessor<LevelUpEvent>
{
    private readonly ICharacterAlgorithm _characterAlgorithm;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IServerManager _serverManager;
    private readonly ISkillsManager _skillsManager;
    private readonly ISpPartnerConfiguration _spPartnerConfiguration;
    private readonly IEnumerable<LevelRewardsImportFile> _levelRewardsConfigurations;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IItemsManager _itemsManager;
    private readonly IBuffFactory _buffFactory;

    public LevelUpEventHandler(IServerManager serverManager,
        IGameLanguageService gameLanguage, ICharacterAlgorithm characterAlgorithm, ISkillsManager skillsManager, ISpPartnerConfiguration spPartnerConfiguration,
        IEnumerable<LevelRewardsImportFile> levelRewardsConfigurations, IGameItemInstanceFactory gameItemInstanceFactory, IItemsManager itemsManager, IBuffFactory buffFactory)
    {
        _serverManager = serverManager;
        _gameLanguage = gameLanguage;
        _characterAlgorithm = characterAlgorithm;
        _skillsManager = skillsManager;
        _spPartnerConfiguration = spPartnerConfiguration;
        _levelRewardsConfigurations = levelRewardsConfigurations;
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _itemsManager = itemsManager;
        _buffFactory = buffFactory;
    }

    public async Task HandleAsync(LevelUpEvent e, CancellationToken cancellation)
    {
        IPlayerEntity character = e.Sender.PlayerEntity;

        switch (e.LevelType)
        {
            case LevelType.Level:
                await HandleLevelUp(character, e.Sender);
                foreach (LevelRewardsImportFile file in _levelRewardsConfigurations)
                {
                    if (character.Class != file.ClassType)
                    {
                        continue;
                    }

                    foreach (LevelRewardsObject obj in file.Items)
                    {
                        if (character.Level != obj.LevelValue || e.LevelType != obj.LevelType)
                        {
                            continue;
                        }

                        foreach (LevelRewardsItem item in obj.ItemsRewards)
                        {
                            if (!character.HasSpaceFor(item.ItemVnum, (short)item.Quantity))
                            {
                                GameItemInstance mailItem = _gameItemInstanceFactory.CreateItem(item.ItemVnum, item.Quantity);
                                await character.Session.EmitEventAsync(new MailCreateEvent(character.Name, character.Id, MailGiftType.Normal, mailItem));
                            }
                            else
                            {
                                string itemName = _itemsManager.GetItem(item.ItemVnum).GetItemName(_gameLanguage, character.Session.UserLanguage);
                                int amount = item.Quantity;
                                character.Session.SendChatMessage(character.Session.GetLanguageFormat(GameDialogKey.YOU_GET_NEW_ITEM, itemName, amount), ChatMessageColorType.Green);
                                await character.Session.AddNewItemToInventory(_gameItemInstanceFactory.CreateItem(item.ItemVnum, item.Quantity, item.Upgrade, (sbyte)item.Rarity));
                            }
                        }
                    }
                }
                break;
            case LevelType.JobLevel:
                await HandleJobLevelUp(character, e.Sender);
                foreach (LevelRewardsImportFile file in _levelRewardsConfigurations)
                {
                    if (character.Class != file.ClassType)
                    {
                        continue;
                    }

                    foreach (LevelRewardsObject obj in file.Items)
                    {
                        if (character.Level != obj.LevelValue || e.LevelType != obj.LevelType)
                        {
                            continue;
                        }

                        foreach (LevelRewardsItem item in obj.ItemsRewards)
                        {
                            if (!character.HasSpaceFor(item.ItemVnum, (short)item.Quantity))
                            {
                                GameItemInstance mailItem = _gameItemInstanceFactory.CreateItem(item.ItemVnum, item.Quantity);
                                await character.Session.EmitEventAsync(new MailCreateEvent(character.Name, character.Id, MailGiftType.Normal, mailItem));
                            }
                            else
                            {
                                string itemName = _itemsManager.GetItem(item.ItemVnum).GetItemName(_gameLanguage, character.Session.UserLanguage);
                                int amount = item.Quantity;
                                character.Session.SendChatMessage(character.Session.GetLanguageFormat(GameDialogKey.YOU_GET_NEW_ITEM, itemName, amount), ChatMessageColorType.Green);
                                await character.Session.AddNewItemToInventory(_gameItemInstanceFactory.CreateItem(item.ItemVnum, item.Quantity, item.Upgrade, (sbyte)item.Rarity));
                            }
                        }
                    }
                }
                break;
            case LevelType.SpJobLevel:
                await HandleSpJobLevelUp(character, e.Sender);
                break;
            case LevelType.Heroic:
                await HandleHeroicLevelUp(character, e.Sender);
                foreach (LevelRewardsImportFile file in _levelRewardsConfigurations)
                {
                    if (character.Class != file.ClassType)
                    {
                        continue;
                    }

                    foreach (LevelRewardsObject obj in file.Items)
                    {
                        if (character.Level != obj.LevelValue || e.LevelType != obj.LevelType)
                        {
                            continue;
                        }

                        foreach (LevelRewardsItem item in obj.ItemsRewards)
                        {
                            if (!character.HasSpaceFor(item.ItemVnum, (short)item.Quantity))
                            {
                                GameItemInstance mailItem = _gameItemInstanceFactory.CreateItem(item.ItemVnum, item.Quantity);
                                await character.Session.EmitEventAsync(new MailCreateEvent(character.Name, character.Id, MailGiftType.Normal, mailItem));
                            }
                            else
                            {
                                string itemName = _itemsManager.GetItem(item.ItemVnum).GetItemName(_gameLanguage, character.Session.UserLanguage);
                                int amount = item.Quantity;
                                character.Session.SendChatMessage(character.Session.GetLanguageFormat(GameDialogKey.YOU_GET_NEW_ITEM, itemName, amount), ChatMessageColorType.Green);
                                await character.Session.AddNewItemToInventory(_gameItemInstanceFactory.CreateItem(item.ItemVnum, item.Quantity, item.Upgrade, (sbyte)item.Rarity));
                            }
                        }
                    }
                }
                break;
            case LevelType.Fairy:
                await HandleFairyLevelUp(character, e.Sender);
                break;
        }
        
        e.Sender.RefreshLevel(_characterAlgorithm);
    }
    
    private async Task HandleLevelUp(IPlayerEntity character, IClientSession session)
    {
        while (character.LevelXp >= _characterAlgorithm.GetLevelXp(character.Level))
        {
            character.LevelXp -= _characterAlgorithm.GetLevelXp(character.Level);
            
            character.Level++;
            
            if (character.Level >= _serverManager.MaxLevel)
            {
                character.Level = (byte)_serverManager.MaxLevel;
                character.LevelXp = 0;
                break;
            }
            
            if (character.Level == _serverManager.HeroicStartLevel && character.HeroLevel == 0)
            {
                character.HeroLevel = 1;
                character.HeroXp = 0;
            }
            
            character.Session.RefreshStatChar();
            character.Hp = character.MaxHp;
            character.Mp = character.MaxMp;
            character.Session.RefreshStat();
            
            switch (character.Level)
            {
                case > 20 when (character.Level % 10) == 0:
                    await session.FamilyAddLogAsync(FamilyLogType.LevelUp, character.Name, character.Level.ToString());
                    await session.FamilyAddExperience(character.Level * 20, FamXpObtainedFromType.LevelUp);
                    break;
                
                case >= 81 and <= 84:
                    await session.FamilyAddLogAsync(FamilyLogType.LevelUp, character.Name, character.Level.ToString());
                    await character.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.TART_HAPENDAM_NO_HERO, character));
                    break;
                
                case > 84:
                    await session.FamilyAddLogAsync(FamilyLogType.LevelUp, character.Name, character.Level.ToString());
                    break;
            }

        }
        
        session.SendLevelUp();
        session.RefreshGroupLevelUi(_spPartnerConfiguration);
        session.SendMsg(
            _gameLanguage.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_LEVELUP, session.UserLanguage),
            MsgMessageType.Middle);
        session.BroadcastEffectInRange(EffectType.NormalLevelUp);
        session.BroadcastEffectInRange(EffectType.NormalLevelUpSubEffect);
    }
    
    private async Task HandleJobLevelUp(IPlayerEntity character, IClientSession session)
    {
        bool isAdventurer = character.Class == ClassType.Adventurer;
        
        while (character.JobLevelXp >= _characterAlgorithm.GetJobXp(character.JobLevel, isAdventurer))
        {
            character.JobLevelXp -= _characterAlgorithm.GetJobXp(character.JobLevel, isAdventurer);
            character.JobLevel++;
            
            if (character.JobLevel >= 20 && isAdventurer)
            {
                character.JobLevel = 20;
                character.JobLevelXp = 0;
                break;
            }
            
            if (character.JobLevel < _serverManager.MaxJobLevel)
            {
                continue;
            }
            
            character.JobLevel = (byte)_serverManager.MaxJobLevel;
            character.JobLevelXp = 0;
            break;
        }
        
        session.SendLevelUp();
        session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_JOB_LEVELUP, session.UserLanguage), MsgMessageType.Middle);
        session.BroadcastEffectInRange(EffectType.JobLevelUp);
        character.SkillComponent.ResetSkillCooldowns = DateTime.UtcNow;
    }
    
    private async Task HandleHeroicLevelUp(IPlayerEntity character, IClientSession session)
    {
        while (character.HeroXp >= _characterAlgorithm.GetHeroLevelXp(character.HeroLevel))
        {
            character.HeroXp -= _characterAlgorithm.GetHeroLevelXp(character.HeroLevel);
            character.HeroLevel++;
            
            if (character.HeroLevel < _serverManager.MaxHeroLevel)
            {
                continue;
            }
            
            character.HeroLevel = (byte)_serverManager.MaxHeroLevel;
            character.HeroXp = 0;
            break;
        }
        
        if (character.HeroLevel != 0 && character.HeroLevel < 30)
        {
            await character.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.TART_HAPENDAM_HERO, character));
        }
        
        character.Hp = character.MaxHp;
        character.Mp = character.MaxMp;
        character.Session.RefreshStat();
        
        switch (character.HeroLevel)
        {
            case > 1 when (character.HeroLevel % 10) == 0:
                await session.FamilyAddLogAsync(FamilyLogType.HeroLevelUp, character.Name, character.HeroLevel.ToString());
                await session.FamilyAddExperience(character.HeroLevel * 20, FamXpObtainedFromType.LevelUp);
                break;
            case > 50:
                await session.FamilyAddLogAsync(FamilyLogType.HeroLevelUp, character.Name, character.HeroLevel.ToString());
                break;
        }
        
        session.SendLevelUp();
        session.RefreshGroupLevelUi(_spPartnerConfiguration);
        session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_HERO_LEVELUP, session.UserLanguage), MsgMessageType.Middle);
        session.BroadcastEffectInRange(EffectType.NormalLevelUp);
        session.BroadcastEffectInRange(EffectType.NormalLevelUpSubEffect);
    }
    
    private async Task HandleSpJobLevelUp(IPlayerEntity character, IClientSession session)
    {
        while (character.Specialist.Xp >= _characterAlgorithm.GetSpecialistJobXp(character.Specialist.SpLevel, character.Specialist.IsFunSpecialist()))
        {
            character.Specialist.Xp -= _characterAlgorithm.GetSpecialistJobXp(character.Specialist.SpLevel, character.Specialist.IsFunSpecialist());
            character.Specialist.SpLevel++;
            
            if (character.Specialist.SpLevel < _serverManager.MaxSpLevel)
            {
                continue;
            }
            
            character.Specialist.SpLevel = (byte)_serverManager.MaxSpLevel;
            character.Specialist.Xp = 0;
            break;
        }
        
        session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SPECIALIST_SHOUTMESSAGE_LEVELUP, session.UserLanguage), MsgMessageType.Middle);
        session.BroadcastEffectInRange(EffectType.JobLevelUp);
        character.SkillComponent.ResetSpSkillCooldowns = DateTime.UtcNow;
    }
    
    private async Task HandleFairyLevelUp(IPlayerEntity character, IClientSession session)
    {
        GameItemInstance fairy = character.Fairy;
        if (fairy == null)
        {
            return;
        }
        
        int fairyXpNeeded = _characterAlgorithm.GetFairyXp((short)(fairy.ElementRate + fairy.GameItem.ElementRate));
        
        while (fairy.Xp >= fairyXpNeeded)
        {
            fairy.Xp -= fairyXpNeeded;
            fairy.ElementRate++;
            
            if ((fairy.ElementRate + fairy.GameItem.ElementRate) == fairy.GameItem.MaxElementRate)
            {
                fairy.Xp = 0;
                session.SendMsg(_gameLanguage.GetLanguageFormat(GameDialogKey.INFORMATION_SHOUTMESSAGE_FAIRYMAX, session.UserLanguage, _gameLanguage.GetLanguage(GameDataType.Item, fairy.GameItem.Name, session.UserLanguage)), MsgMessageType.Middle);
                break;
            }
            fairyXpNeeded = _characterAlgorithm.GetFairyXp((short)(fairy.ElementRate + fairy.GameItem.ElementRate));
        }
        
        session.SendMsg(_gameLanguage.GetLanguageFormat(GameDialogKey.INFORMATION_SHOUTMESSAGE_FAIRY_LEVELUP, session.UserLanguage, _gameLanguage.GetLanguage(GameDataType.Item, fairy.GameItem.Name, session.UserLanguage)), MsgMessageType.Middle);
        session.RefreshFairy();
    }
}