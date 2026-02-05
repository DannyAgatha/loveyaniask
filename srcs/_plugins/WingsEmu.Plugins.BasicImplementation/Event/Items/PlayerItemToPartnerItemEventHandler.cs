using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._enum;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Items;

public class PlayerItemToPartnerItemEventHandler : IAsyncEventProcessor<PlayerItemToPartnerItemEvent>
{
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

    public async Task HandleAsync(PlayerItemToPartnerItemEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        InventoryItem item = session.PlayerEntity.GetItemBySlotAndType(e.Slot, e.InventoryType);
        if (item == null)
        {
            return;
        }

        if (item.ItemInstance.Type != ItemInstanceType.WearableInstance)
        {
            return;
        }
        
        if (item.ItemInstance.CarvedRunes.Upgrade > 0)
        {
            session.SendInfoi2(Game18NConstString.YouCantTradeRuneWeaponsForPartnerEquipment);
            return;
        }

        GameItemInstance itemToTransform = item.ItemInstance;

        const ItemVnums donaVNum = ItemVnums.DONA_RIVER_SAND;
        int price = 300 * itemToTransform.GameItem.LevelMinimum + 2000;

        if (itemToTransform.GameItem.EquipmentSlot != EquipmentType.Armor
            && itemToTransform.GameItem.EquipmentSlot != EquipmentType.MainWeapon
            && itemToTransform.GameItem.EquipmentSlot != EquipmentType.SecondaryWeapon)
        {
            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.NORMAL, "Tried to transform a PlayerItem to a PartnerItem, and that PlayerItem is not the 'transformable' type" +
                "(in theory there is a Client-side check for that)");
            return;
        }

        if (PartnerItemsWeapons.Contains(itemToTransform.ItemVNum) || PartnerItemsArmors.Contains(itemToTransform.ItemVNum))
        {
            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.NORMAL, "Tried to transform a PartnerItem to a PartnerItem (not possible with normal client)");
            return;
        }

        if (session.PlayerEntity.Gold < price)
        {
            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "Seems like the balance the client thinks he has is superior to what the server has. (not sufficient" +
                " gold to pay the PlayerItem to PartnerItem transformation)");
            return;
        }

        if (!session.PlayerEntity.HasItem((short)donaVNum, itemToTransform.GameItem.LevelMinimum))
        {
            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "Tried to transform a PlayerItem to a PartnerItem without sufficient amount of" +
                $" 'Dona River Sand' ({((short)donaVNum).ToString()}), and the Client-side check should have prevented that.");
            return;
        }

        if (itemToTransform.EquipmentOptions != null && itemToTransform.EquipmentOptions.Count != 0)
        {
            itemToTransform.EquipmentOptions.Clear();
            itemToTransform.ShellRarity = null;
        }

        switch (itemToTransform.GameItem.EquipmentSlot)
        {
            case EquipmentType.Armor:
                switch (itemToTransform.GameItem.Class)
                {
                    case (int)ItemClassType.Swordsman:
                    case (int)ItemClassType.MartialArtist:
                        itemToTransform.ItemVNum = itemToTransform.GameItem.IsHeroic ? (int)ItemVnums.PARTNER_ARMOR_CHAMPION_MELEE : (int)ItemVnums.PARTNER_ARMOR_MELEE;
                        break;
                    case (int)ItemClassType.Archer:
                        itemToTransform.ItemVNum = itemToTransform.GameItem.IsHeroic ? (int)ItemVnums.PARTNER_ARMOR_CHAMPION_RANGED : (int)ItemVnums.PARTNER_ARMOR_RANGED;
                        break;
                    case (int)ItemClassType.Mage:
                        itemToTransform.ItemVNum = itemToTransform.GameItem.IsHeroic ? (int)ItemVnums.PARTNER_ARMOR_CHAMPION_MAGIC : (int)ItemVnums.PARTNER_ARMOR_MAGIC;
                        break;
                    default:
                        session.SendShopEndPacket(ShopEndType.Npc);
                        return;
                }

                break;
            case EquipmentType.SecondaryWeapon:
            case EquipmentType.MainWeapon:
                switch (itemToTransform.GameItem.Class)
                {
                    case (int)ItemClassType.Swordsman:
                        itemToTransform.ItemVNum = itemToTransform.GameItem.IsHeroic ? (int)ItemVnums.PARTNER_WEAPON_CHAMPION_MELEE : 
                            itemToTransform.GameItem.EquipmentSlot == (int)EquipmentType.MainWeapon ? (int)ItemVnums.PARTNER_WEAPON_MELEE : (int)ItemVnums.PARTNER_WEAPON_RANGED;
                        break;
                    case (int)ItemClassType.Archer:
                        itemToTransform.ItemVNum = itemToTransform.GameItem.IsHeroic ? (int)ItemVnums.PARTNER_WEAPON_CHAMPION_RANGED : 
                            itemToTransform.GameItem.EquipmentSlot == (int)EquipmentType.MainWeapon ? (int)ItemVnums.PARTNER_WEAPON_RANGED : (int)ItemVnums.PARTNER_WEAPON_MELEE;
                        break;
                    case (int)ItemClassType.Mage:
                        itemToTransform.ItemVNum = itemToTransform.GameItem.IsHeroic ? (int)ItemVnums.PARTNER_WEAPON_CHAMPION_MAGIC : 
                            itemToTransform.GameItem.EquipmentSlot == (int)EquipmentType.MainWeapon ? (int)ItemVnums.PARTNER_WEAPON_MAGIC : (int)ItemVnums.PARTNER_WEAPON_RANGED;
                        break;
                    case (int)ItemClassType.MartialArtist:
                        itemToTransform.ItemVNum = itemToTransform.GameItem.IsHeroic ? (int)ItemVnums.PARTNER_WEAPON_CHAMPION_MELEE : (int)ItemVnums.PARTNER_WEAPON_MELEE;
                        break;
                    default:
                        session.SendShopEndPacket(ShopEndType.Npc);
                        return;
                }

                break;
            default:
                session.SendShopEndPacket(ShopEndType.Npc);
                return;
        }

        if (itemToTransform.Type == ItemInstanceType.WearableInstance)
        {
            itemToTransform.EquipmentOptions?.Clear();
            itemToTransform.OriginalItemVnum = item.ItemInstance.GameItem.Id;
            itemToTransform.BoundCharacterId = null;
        }

        await session.RemoveItemFromInventory((short)donaVNum, itemToTransform.GameItem.LevelMinimum);
        session.PlayerEntity.Gold -= price;
        InventoryItem getItem = session.PlayerEntity.GetItemBySlotAndType(e.Slot, e.InventoryType);
        await session.RemoveItemFromInventory(item: getItem);
        await session.AddNewItemToInventory(itemToTransform);
        session.RefreshGold();
        session.SendShopEndPacket(ShopEndType.Npc);
    }
}