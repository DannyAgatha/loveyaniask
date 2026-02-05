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
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums.Families;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class SetLevelEventHandler : IAsyncEventProcessor<SetLevelEvent>
{
    private readonly ICharacterAlgorithm _characterAlgorithm;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IServerManager _serverManager;
    private readonly ISpPartnerConfiguration _spPartnerConfiguration;
    private readonly IEnumerable<LevelRewardsImportFile> _levelRewardsConfigurations;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IItemsManager _itemsManager;
    private readonly IBuffFactory _buffFactory;

    public SetLevelEventHandler(IServerManager serverManager,
        IGameLanguageService gameLanguage, ICharacterAlgorithm characterAlgorithm, ISkillsManager skillsManager, ISpPartnerConfiguration spPartnerConfiguration,
        IEnumerable<LevelRewardsImportFile> levelRewardsConfigurations, IGameItemInstanceFactory gameItemInstanceFactory, IItemsManager itemsManager, IBuffFactory buffFactory)
    {
        _serverManager = serverManager;
        _gameLanguage = gameLanguage;
        _characterAlgorithm = characterAlgorithm;
        _spPartnerConfiguration = spPartnerConfiguration;
        _levelRewardsConfigurations = levelRewardsConfigurations;
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _itemsManager = itemsManager;
        _buffFactory = buffFactory;
    }

    public async Task HandleAsync(SetLevelEvent e, CancellationToken cancellation)
    {
        IPlayerEntity character = e.Sender.PlayerEntity;

        switch (e.LevelType)
        {
            case LevelType.Level:
                await HandleLevelUp(e.Level, e.Sender);
                
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
                HandleJobLevelUp(e.Level, e.Sender);
                
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
        }

        e.Sender.RefreshLevel(_characterAlgorithm);
    }

    private async Task HandleLevelUp(byte level, IClientSession session)
    {
        IPlayerEntity character = session.PlayerEntity;
        character.LevelXp = 0;
        character.Level = level;

        if (character.Level >= _serverManager.MaxLevel)
        {
            character.Level = (byte)_serverManager.MaxLevel;
            character.LevelXp = 0;
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
            case > 80:
                await session.FamilyAddLogAsync(FamilyLogType.LevelUp, character.Name, character.Level.ToString());

                if (character.Level < 85)
                {
                    await character.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.TART_HAPENDAM_NO_HERO, character));
                }
                
                break;
        }

        session.SendLevelUp();
        session.RefreshGroupLevelUi(_spPartnerConfiguration);
        session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_LEVELUP, session.UserLanguage), MsgMessageType.Middle);
        session.BroadcastEffectInRange(EffectType.NormalLevelUp);
        session.BroadcastEffectInRange(EffectType.NormalLevelUpSubEffect);
    }

    private void HandleJobLevelUp(byte level, IClientSession session)
    {
        IPlayerEntity character = session.PlayerEntity;
        character.JobLevelXp = 0;
        character.JobLevel = level;

        if (character.JobLevel >= 20 && character.Class == ClassType.Adventurer)
        {
            character.JobLevel = 20;
            character.JobLevelXp = 0;
        }
        else if (character.JobLevel >= _serverManager.MaxJobLevel)
        {
            character.JobLevel = (byte)_serverManager.MaxJobLevel;
            character.JobLevelXp = 0;
        }
        session.SendLevelUp();
        session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_JOB_LEVELUP, session.UserLanguage), MsgMessageType.Middle);
        session.BroadcastEffectInRange(EffectType.JobLevelUp);
        character.SkillComponent.ResetSkillCooldowns = DateTime.UtcNow;
    }
}