using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Mails;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Mails.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class ChangeClassByItemEventHandler : IAsyncEventProcessor<ChangeClassByItemEvent>
{
    private readonly ChangeClassConfiguration _changeClassConfiguration;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IRandomGenerator _randomGenerator;

    public ChangeClassByItemEventHandler(ChangeClassConfiguration changeClassConfiguration, IGameItemInstanceFactory gameItemInstanceFactory, IRandomGenerator randomGenerator)
    {
        _changeClassConfiguration = changeClassConfiguration;
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _randomGenerator = randomGenerator;
    }
    
    private static readonly EquipmentType[] EquipmentTypes = { EquipmentType.MainWeapon, EquipmentType.SecondaryWeapon, EquipmentType.Armor, EquipmentType.Sp, EquipmentType.WeaponSkin };

    public async Task HandleAsync(ChangeClassByItemEvent e, CancellationToken cancellation)
    {
        ClassType newClassType = e.NewClassType;
        InventoryItem gameItemInstance = e.ItemInstance;
        IClientSession session = e.Sender;
        IPlayerEntity playerEntity = session.PlayerEntity;
        bool confirm = e.Confirmation;

        if (gameItemInstance is null)
        {
            return;
        }

        if (playerEntity.Class == newClassType || playerEntity.Class == ClassType.Adventurer || newClassType == ClassType.Adventurer)
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.CLASS_CHANGE_CHATMESSAGE_WRONG_CLASS_TYPE), ChatMessageColorType.Yellow);
            return;
        }
        
        if (session.PlayerEntity.HasShopOpened || session.PlayerEntity.ShopComponent.Items != null)
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_USE), ChatMessageColorType.Yellow);
            return;
        }
        
        if (!session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_MUST_BE_IN_CLASSIC_MAP), MsgMessageType.Middle);
            return;
        }

        if (playerEntity.IsOnVehicle)
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_REMOVE_VEHICLE), MsgMessageType.Middle);
            return;
        }
        
        if (playerEntity.UseSp || playerEntity.IsMorphed)
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.CLASS_CHANGE_CHATMESSAGE_SPECIALIST_UNTRANSFORM), ChatMessageColorType.Yellow);
            return;
        }
        
        if (session.PlayerEntity.BuffComponent.HasBuff(BuffGroup.Bad))
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.SPECIALIST_SHOUTMESSAGE_NO_REMOVE_DEBUFFS), MsgMessageType.Middle);
            return;
        }

        if (!confirm)
        {
            session.SendQnaPacket($"u_i 1 {playerEntity.Id} {gameItemInstance.ItemInstance.GameItem.Type} {gameItemInstance.Slot} 1", session.GetLanguage(GameDialogKey.CLASS_CHANGE_ASK_CONFIRM));
            return;
        }
        
        ChangeClassByTypeConfig currentClassConfig = _changeClassConfiguration.GetConfigByClassType(playerEntity.Class);
        if (currentClassConfig is null)
        {
            return;
        }

        ChangeClassByTypeConfig newClassConfig = _changeClassConfiguration.GetConfigByClassType(newClassType);
        if (newClassConfig is null)
        {
            return;
        }

        session.PlayerEntity.LastSkillUse = DateTime.MinValue;
        
        int ordinal = session.PlayerEntity.SubClass switch
        {
            SubClassType.OathKeeper or SubClassType.SilentStalker or SubClassType.ArcaneSage or SubClassType.ZenWarrior => 1,
            SubClassType.CrimsonFury or SubClassType.ArrowLord or SubClassType.Pyromancer or SubClassType.EmperorsBlade => 2,
            SubClassType.CelestialPaladin or SubClassType.ShadowHunter or SubClassType.DarkNecromancer or SubClassType.StealthShadow => 3,
            _ => 0
        };
        
        SubClassType newSubClass = (newClassType, ordinal) switch
        {
            (ClassType.Swordman, 1) => SubClassType.OathKeeper,
            (ClassType.Swordman, 2) => SubClassType.CrimsonFury,
            (ClassType.Swordman, 3) => SubClassType.CelestialPaladin,
            (ClassType.Archer, 1) => SubClassType.SilentStalker,
            (ClassType.Archer, 2) => SubClassType.ArrowLord,
            (ClassType.Archer, 3) => SubClassType.ShadowHunter,
            (ClassType.Magician, 1) => SubClassType.ArcaneSage,
            (ClassType.Magician, 2) => SubClassType.Pyromancer,
            (ClassType.Magician, 3) => SubClassType.DarkNecromancer,
            (ClassType.MartialArtist, 1) => SubClassType.ZenWarrior,
            (ClassType.MartialArtist, 2) => SubClassType.EmperorsBlade,
            (ClassType.MartialArtist, 3) => SubClassType.StealthShadow,
            _ => SubClassType.NotDefined
        };

        foreach (EquipmentType equipmentType in EquipmentTypes)
        {
            switch (equipmentType)
            {
                case EquipmentType.MainWeapon:
                    GameItemInstance mainWeapon = session.PlayerEntity.MainWeapon;
                    if (mainWeapon is not null)
                    {
                        bool isHero = mainWeapon.GameItem.IsHeroic;
                        byte level = mainWeapon.GameItem.LevelMinimum;
                        ChangeClassConfigItem mainWeaponConfig = newClassConfig.MainWeapons.FirstOrDefault(x => isHero ? x.HeroLevel == level : x.Level == level);
                        if (mainWeaponConfig is not null)
                        {
                            GameItemInstance newMainWeapon = CreateNewDuplicatedItem(session, mainWeapon, mainWeaponConfig);
                            await session.EmitEventAsync(new MailCreateEvent(session.PlayerEntity.Name, session.PlayerEntity.Id, MailGiftType.Normal, newMainWeapon));
                        }
                    }
                    break;
                case EquipmentType.SecondaryWeapon:
                    GameItemInstance secondWeapon = session.PlayerEntity.SecondaryWeapon;
                    if (secondWeapon is not null)
                    {
                        bool isHero = secondWeapon.GameItem.IsHeroic;
                        byte level = secondWeapon.GameItem.LevelMinimum;
                        ChangeClassConfigItem secondWeaponConfig = newClassConfig.SecondWeapons.FirstOrDefault(x => isHero ? x.HeroLevel == level : x.Level == level);
                        if (secondWeaponConfig is not null)
                        {
                            GameItemInstance newSecondWeapon = CreateNewDuplicatedItem(session, secondWeapon, secondWeaponConfig);
                            await session.EmitEventAsync(new MailCreateEvent(session.PlayerEntity.Name, session.PlayerEntity.Id, MailGiftType.Normal, newSecondWeapon));
                        }
                    }
                    break;
                case EquipmentType.Armor:
                    GameItemInstance armor = session.PlayerEntity.Armor;
                    if (armor is not null)
                    {
                        bool isHero = armor.GameItem.IsHeroic;
                        byte level = armor.GameItem.LevelMinimum;
                        ChangeClassConfigItem armorConfig = newClassConfig.Armors.FirstOrDefault(x => isHero ? x.HeroLevel == level : x.Level == level);
                        if (armorConfig is not null)
                        {
                            GameItemInstance newArmor = CreateNewDuplicatedItem(session, armor, armorConfig);
                            await session.EmitEventAsync(new MailCreateEvent(session.PlayerEntity.Name, session.PlayerEntity.Id, MailGiftType.Normal, newArmor));
                        }
                    }
                    break;
                case EquipmentType.Sp:
                    IReadOnlyList<InventoryItem> specialists = playerEntity.GetItemsByInventoryType(InventoryType.Specialist).ToList();
                    foreach (InventoryItem specialist in specialists)
                    {
                        if (specialist is null || specialist.ItemInstance.GameItem.IsPartnerSpecialist)
                        {
                            continue;
                        }

                        ChangeClassConfigItem specialistConfig = currentClassConfig.Specialists.FirstOrDefault(x => x.ItemVnum == specialist.ItemInstance.ItemVNum);
                        if (specialistConfig is null)
                        {
                            continue;
                        }
                        
                        ChangeClassConfigItem newSpecialistConfig = newClassConfig.Specialists.FirstOrDefault(x => x.Type is not null && x.Type == specialistConfig.Type);
                        if (newSpecialistConfig is null)
                        {
                            continue;
                        }

                        GameItemInstance newSpecialist = CreateNewDuplicatedItem(session, specialist.ItemInstance, newSpecialistConfig);
                        await session.EmitEventAsync(new MailCreateEvent(session.PlayerEntity.Name, session.PlayerEntity.Id, MailGiftType.Normal, newSpecialist));
                        await session.RemoveItemFromInventory(item: specialist);
                    }

                    GameItemInstance equippedSpecialist = playerEntity.Specialist;
                    if (equippedSpecialist is not null)
                    {
                        ChangeClassConfigItem specialistConfig = currentClassConfig.Specialists.FirstOrDefault(x => x.ItemVnum == equippedSpecialist.ItemVNum);
                        if (specialistConfig is not null)
                        {
                            ChangeClassConfigItem newSpecialistConfig = newClassConfig.Specialists.FirstOrDefault(x => x.Type is not null && x.Type == specialistConfig.Type);
                            if (newSpecialistConfig is not null)
                            {
                                GameItemInstance newSpecialist = CreateNewDuplicatedItem(session, equippedSpecialist, newSpecialistConfig);
                                await session.EmitEventAsync(new MailCreateEvent(session.PlayerEntity.Name, session.PlayerEntity.Id, MailGiftType.Normal, newSpecialist));
                            }
                        }
                    }
                    break;
                
                case EquipmentType.WeaponSkin:
                    GameItemInstance weaponSkin = session.PlayerEntity.WeaponSkin;
                    if (weaponSkin is not null)
                    {
                        string? currentCategory = currentClassConfig.WeaponSkins
                            .FirstOrDefault(x => x.ItemVnum == weaponSkin.ItemVNum)?.Category;

                        if (currentCategory is not null)
                        {
                            ChangeClassConfigItem newWeaponSkinConfig = newClassConfig.WeaponSkins
                                .FirstOrDefault(x => x.Category == currentCategory);

                            if (newWeaponSkinConfig is not null)
                            {
                                GameItemInstance newWeaponSkin = CreateNewDuplicatedItem(session, weaponSkin, newWeaponSkinConfig);
                                await session.EmitEventAsync(new MailCreateEvent(session.PlayerEntity.Name, session.PlayerEntity.Id, MailGiftType.Normal, newWeaponSkin));
                            }
                        }
                    }
                    break;


            }
            
            await session.EmitEventAsync(new InventoryTakeOffItemEvent((byte)equipmentType)
            {
                ForceToRandomSlot = true,
            });

            InventoryItem oldItem = session.PlayerEntity.GetItemBySlotAndType(short.MaxValue, InventoryType.Equipment);
            if (oldItem is null)
            {
                continue;
            }
            
            await session.RemoveItemFromInventory(item: oldItem);
        }

        await session.EmitEventAsync(new ChangeClassEvent
        {
            NewClass = newClassType,
        });
        
        await session.EmitEventAsync(new ChangeSubClassEvent
        {
            NewSubClass = newSubClass,
            TierLevel = session.PlayerEntity.TierLevel,
            TierExperience = session.PlayerEntity.TierExperience
        });
        
        await session.RemoveItemFromInventory(item: gameItemInstance);
    }

    private GameItemInstance CreateNewDuplicatedItem(IClientSession session, GameItemInstance oldItem, ChangeClassConfigItem configItem)
    {
        GameItemInstance newItem = _gameItemInstanceFactory.DuplicateItem(oldItem);
        newItem.SerialTracker = Guid.NewGuid();
        newItem.ItemVNum = configItem.ItemVnum;
        
        newItem.SetRarityPoint(_randomGenerator);

        return newItem;
    }
}