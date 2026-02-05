using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Inventory;

public class PartnerInventoryEquipItemEventHandler : IAsyncEventProcessor<PartnerInventoryEquipItemEvent>
{
    private static readonly HashSet<EquipmentType> EquipmentTypes = new()
    {
        EquipmentType.Armor, EquipmentType.MainWeapon, EquipmentType.Sp, EquipmentType.Gloves, EquipmentType.Boots, EquipmentType.SecondaryWeapon
    };

    private static readonly int[] PartnerItemsWeapons =
    {
        (int)ItemVnums.PARTNER_WEAPON_MELEE, (int)ItemVnums.PARTNER_WEAPON_RANGED, (int)ItemVnums.PARTNER_WEAPON_MAGIC,
        (int)ItemVnums.PARTNER_WEAPON_CHAMPION_MELEE, (int)ItemVnums.PARTNER_WEAPON_CHAMPION_RANGED, (int)ItemVnums.PARTNER_WEAPON_CHAMPION_MAGIC
    };

    private static readonly int[] PartnerItemsArmors =
    {
        (int)ItemVnums.PARTNER_ARMOR_MAGIC, (int)ItemVnums.PARTNER_ARMOR_RANGED, (int)ItemVnums.PARTNER_ARMOR_MELEE,
        (int)ItemVnums.PARTNER_ARMOR_CHAMPION_MAGIC, (int)ItemVnums.PARTNER_ARMOR_CHAMPION_RANGED, (int)ItemVnums.PARTNER_ARMOR_CHAMPION_MELEE,
    };

    private readonly IGameLanguageService _gameLanguage;

    public PartnerInventoryEquipItemEventHandler(IGameLanguageService gameLanguage) => _gameLanguage = gameLanguage;

    public async Task HandleAsync(PartnerInventoryEquipItemEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        if (session.PlayerEntity.IsInExchange() || !session.HasCurrentMapInstance)
        {
            return;
        }

        if (session.PlayerEntity.HasShopOpened || session.PlayerEntity.ShopComponent.Items != null)
        {
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_USE, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        short petId = (short)(e.PartnerSlot - 1);
        byte slot = e.Slot;

        IMateEntity mateEntity = session.PlayerEntity.MateComponent.GetMate(x => x.PetSlot == petId && x.MateType == MateType.Partner);
        if (mateEntity == null)
        {
            return;
        }

        InventoryItem inv = session.PlayerEntity.GetItemBySlotAndType(slot, e.InventoryType);
        if (inv == null)
        {
            return;
        }

        short invSlot = inv.Slot;

        if (inv.ItemInstance.Type != ItemInstanceType.WearableInstance && inv.ItemInstance.Type != ItemInstanceType.SpecialistInstance)
        {
            return;
        }
        
        if (e.InventoryType == InventoryType.Specialist)
        {
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_WEAR_PARTNER, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        GameItemInstance item = inv.ItemInstance;

        EquipmentType equipmentType = item.GameItem.EquipmentSlot;
        if (!EquipmentTypes.Contains(equipmentType))
        {
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_WEAR_PARTNER, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        switch (equipmentType)
        {
            case EquipmentType.MainWeapon:
                if (!PartnerItemsWeapons.Contains(item.ItemVNum))
                {
                    session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_WEAR_PARTNER, session.UserLanguage), ChatMessageColorType.Yellow);
                    return;
                }

                break;
            
            case EquipmentType.SecondaryWeapon:
                if (!PartnerItemsArmors.Contains(item.ItemVNum))
                {
                    session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_WEAR_PARTNER, session.UserLanguage), ChatMessageColorType.Yellow);
                    return;
                }

                break;
            
            case EquipmentType.Armor:
                if (!PartnerItemsArmors.Contains(item.ItemVNum))
                {
                    session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_WEAR_PARTNER, session.UserLanguage), ChatMessageColorType.Yellow);
                    return;
                }

                break;
        }

        ItemType itemType = item.GameItem.ItemType;

        if (equipmentType == EquipmentType.Sp)
        {
            if (!item.GameItem.IsPartnerSpecialist)
            {
                return;
            }

            if (mateEntity.IsUsingSp)
            {
                return;
            }

            if (!mateEntity.CanWearSpecialist(inv.ItemInstance.GameItem))
            {
                session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_WEAR_PARTNER, session.UserLanguage), ChatMessageColorType.Yellow);
                return;
            }

            if (!mateEntity.IsUsingSp && !mateEntity.IsSpCooldownElapsed())
            {
                session.SendMsg(_gameLanguage.GetLanguageFormat(GameDialogKey.SPECIALIST_SHOUTMESSAGE_IN_COOLDOWN, session.UserLanguage, mateEntity.GetSpCooldown()), MsgMessageType.Middle);
                return;
            }
            
            if (mateEntity.Level < 20)
            {
                return;
            }
        }

        if (itemType != ItemType.Weapon && itemType != ItemType.Armor && itemType != ItemType.Specialist && itemType != ItemType.Fashion)
        {
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_WEAR_PARTNER, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }
        
        if (item.GameItem.LevelMinimum > (item.GameItem.IsHeroic ? session.PlayerEntity.Level : mateEntity.Level))
        {
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_WEAR_PARTNER, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        if (!mateEntity.CanWearItem(item.GameItem))
        {
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_WEAR_PARTNER, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        if (equipmentType == EquipmentType.SecondaryWeapon)
        {
            equipmentType = EquipmentType.MainWeapon;
        }

        PartnerInventoryItem equippedItem = session.PlayerEntity.PartnerGetEquippedItem(equipmentType, mateEntity.PetSlot);
        if (equippedItem != null)
        {
            GameItemInstance instance = equippedItem.ItemInstance;

            if (!session.PlayerEntity.HasSpaceFor(instance.ItemVNum, (short)instance.Amount))
            {
                session.SendMsg(session.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_PLACE), MsgMessageType.Middle);
                return;
            }

            session.PlayerEntity.PartnerTakeOffItem(equipmentType, mateEntity.PetSlot);
            await session.AddNewItemToInventory(instance, slot: invSlot, type: e.InventoryType);
            mateEntity.RefreshEquipmentValues(instance, true);
        }

        session.PlayerEntity.PartnerEquipItem(inv, petId);
        session.SendScpPackets();
        session.SendScnPackets();
        await session.RemoveItemFromInventory(item: inv, sendPackets: equippedItem == null);
        mateEntity.RefreshEquipmentValues(item, false);
        session.SendPetInfo(mateEntity, _gameLanguage);
        session.SendCondMate(mateEntity);
    }
}