using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.CharacterSizeModifiers;
using WingsEmu.Game.Configurations.SetEffect;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.BasicImplementations.Inventory;

public class InventoryEquipItemEventHandler : IAsyncEventProcessor<InventoryEquipItemEvent>
{
    private readonly HashSet<EquipmentType> _bindItems = new()
    {
        EquipmentType.CostumeHat,
        EquipmentType.CostumeSuit,
        EquipmentType.WeaponSkin,
        EquipmentType.Fairy
    };

    private readonly HashSet<EquipmentType> _bindEq = new()
    {
        EquipmentType.MainWeapon,
        EquipmentType.Armor,
        EquipmentType.SecondaryWeapon
    };

    private readonly IGameLanguageService _gameLanguage;

    public InventoryEquipItemEventHandler(IGameLanguageService gameLanguage)
    {
        _gameLanguage = gameLanguage;
    }

    public async Task HandleAsync(InventoryEquipItemEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        bool isSpecialType = e.IsSpecialType;
        InventoryType? inventoryType = e.InventoryType;
        bool bindItem = e.BoundItem;

        if (session.PlayerEntity.IsInExchange() || !session.HasCurrentMapInstance)
        {
            return;
        }

        if (session.PlayerEntity.HasShopOpened || session.PlayerEntity.ShopComponent.Items != null)
        {
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_USE, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        InventoryItem inv;
        if (isSpecialType && inventoryType.HasValue)
        {
            inv = session.PlayerEntity.GetItemBySlotAndType(e.Slot, inventoryType.Value);
        }
        else
        {
            inv = session.PlayerEntity.GetItemBySlotAndType(e.Slot, InventoryType.Equipment);
        }

        if (inv == null)
        {
            return;
        }

        if (inv.ItemInstance.Type != ItemInstanceType.WearableInstance && inv.ItemInstance.Type != ItemInstanceType.SpecialistInstance)
        {
            return;
        }

        GameItemInstance item = inv.ItemInstance;
        EquipmentType equipmentType = item.GameItem.EquipmentSlot;
        ItemType itemType = item.GameItem.ItemType;

        if (equipmentType == EquipmentType.Sp)
        {
            if (item.GameItem.IsPartnerSpecialist)
            {
                return;
            }

            if (session.PlayerEntity.UseSp)
            {
                return;
            }

            if (item.Rarity == -2)
            {
                session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.ITEM_SHOUTMESSAGE_CANT_WEAR_SP_DESTROYED, session.UserLanguage), MsgMessageType.Middle);
                return;
            }
        }

        if (itemType != ItemType.Weapon && itemType != ItemType.Armor && itemType != ItemType.Fashion && itemType != ItemType.Jewelry && itemType != ItemType.Specialist)
        {
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_WEAR, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        if (item.GameItem.LevelMinimum > (item.GameItem.IsHeroic ? session.PlayerEntity.HeroLevel : session.PlayerEntity.Level) ||
            item.GameItem.Sex != 0 && item.GameItem.Sex != ((byte)session.PlayerEntity.Gender + 1))
        {
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_WEAR, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        if (itemType != ItemType.Jewelry && equipmentType != EquipmentType.Boots && equipmentType != EquipmentType.Gloves && (item.GameItem.Class >> (byte)session.PlayerEntity.Class & 1) != 1)
        {
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_WEAR, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        GameItemInstance specialist = session.PlayerEntity.Specialist;
        if (session.PlayerEntity.UseSp && specialist != null)
        {
            if (specialist.GameItem.Element != 0 && equipmentType == EquipmentType.Fairy && item.GameItem.Element != specialist.GameItem.Element && !session.IsRenegadeSpecialist(specialist, item) && 
                session.PlayerEntity.Morph != (int)MorphType.PetTrainer && session.PlayerEntity.Morph != (int)MorphType.PetTrainerSkin && 
                session.PlayerEntity.Morph != (int)MorphType.Angler && session.PlayerEntity.Morph != (int)MorphType.AnglerSkin && 
                session.PlayerEntity.Morph != (int)MorphType.Chef && session.PlayerEntity.Morph != (int)MorphType.ChefSkin)
            {
                session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.ITEM_SHOUTMESSAGE_FAIRY_WRONG_ELEMENT, session.UserLanguage), MsgMessageType.Middle);
                return;
            }
        }

        if (itemType is ItemType.Weapon or ItemType.Armor)
        {
            if (item.BoundCharacterId.HasValue && item.BoundCharacterId.Value != session.PlayerEntity.Id)
            {
                session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_WEAR, session.UserLanguage), ChatMessageColorType.Yellow);
                return;
            }
        }

        if (session.PlayerEntity.UseSp && equipmentType == EquipmentType.Sp)
        {
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.SPECIALIST_CHATMESSAGE_SP_BLOCKED, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        if (session.PlayerEntity.JobLevel < item.GameItem.LevelJobMinimum)
        {
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_MESSAGE_LOW_JOB, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        if (item.IsBound && bindItem)
        {
            return;
        }
        
        if (item.GameItem.EquipmentSlot == EquipmentType.MiniPet && !item.IsBound)
        {
            if (!bindItem)
            {
                session.SendQnaiPacket($"wear {inv.Slot} 0 1", Game18NConstString.CannotTradeAskWearing);
                return;
            }
            
            item.BoundCharacterId = session.PlayerEntity.Id;

            if (item.GameItem.ItemValidTime > 0)
            {
                item.ItemDeleteTime = DateTime.UtcNow.AddHours(item.GameItem.ItemValidTime);
            }
        }

        if (!item.IsBound && ((item.GameItem.ItemValidTime != 0 && (_bindItems.Contains(item.GameItem.EquipmentSlot) || item.GameItem.ItemType == ItemType.Jewelry))
                || (_bindEq.Contains(item.GameItem.EquipmentSlot) && item.GameItem.IsHeroic && item.Rarity >= 1 && item.BoundCharacterId == null) || (equipmentType == EquipmentType.Fairy && item.GameItem.MaxElementRate is 70 or 80)))
        {
            switch (bindItem)
            {
                case false when _bindItems.Contains(item.GameItem.EquipmentSlot):
                    session.SendQnaiPacket($"wear {inv.Slot} 0 1", Game18NConstString.CannotTradeAskWearing);
                    return;
                case false when _bindEq.Contains(item.GameItem.EquipmentSlot):
                    session.SendQnaiPacket($"wear {inv.Slot} 0 1", Game18NConstString.CannotTradeAskWearing);
                    return;
            }

            item.BoundCharacterId = session.PlayerEntity.Id;

            item.ItemDeleteTime = item.GameItem.ItemValidTime switch
            {
                -1 => null,
                > 0 => DateTime.UtcNow.AddSeconds(item.GameItem.ItemValidTime),
                _ => item.ItemDeleteTime
            };
        }

        bool buffAmulet = false;
        if ((item.ItemDeleteTime.HasValue || item.DurabilityPoint != 0) && !_bindItems.Contains(item.GameItem.EquipmentSlot))
        {
            session.SendAmuletBuffPacket(item);
            buffAmulet = true;
        }

        bool removeAmuletBuff = false;
        InventoryItem itemInEquipment = session.PlayerEntity.GetInventoryItemFromEquipmentSlot(equipmentType);
        if (itemInEquipment == null)
        {
            session.SendInventoryRemovePacket(inv);
            session.PlayerEntity.EquipItem(inv, equipmentType);
        }
        else
        {
            if ((itemInEquipment.ItemInstance.ItemDeleteTime.HasValue || itemInEquipment.ItemInstance.DurabilityPoint != 0)
                && !_bindItems.Contains(itemInEquipment.ItemInstance.GameItem.EquipmentSlot))
            {
                removeAmuletBuff = true;
            }

            session.PlayerEntity.TakeOffItem(equipmentType, inv.Slot, isSpecialType && inventoryType.HasValue ? inventoryType.Value : InventoryType.Equipment);
            session.PlayerEntity.EquipItem(inv, equipmentType);
            session.PlayerEntity.RefreshEquipmentValues(itemInEquipment.ItemInstance, true);
        }

        if (removeAmuletBuff && !buffAmulet)
        {
            session.SendEmptyAmuletBuffPacket();
        }

        session.PlayerEntity.RefreshEquipmentValues(item);
        session.RefreshStatChar();
        session.RefreshEquipment();
        if (itemInEquipment != null)
        {
            session.SendInventoryAddPacket(itemInEquipment);
        }

        session.BroadcastEq();
        session.SendCondPacket();
        session.RefreshStat();
        session.SendIncreaseRange();
        await session.EmitEventAsync(new CharacterSizeModifierCheckEvent());
        
        switch (equipmentType)
        {
            case EquipmentType.Fairy:
                session.BroadcastPairy();
                break;
            case EquipmentType.Amulet:
                session.BroadcastEffectInRange(EffectType.EquipAmulet);
                break;
            case EquipmentType.MiniPet:
                session.BroadcastMiniPet();
                break;
        }
    }
}