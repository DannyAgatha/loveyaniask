using System;
using System.Collections.Generic;
using System.Linq;
using WingsEmu.DTOs.Bonus;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Warehouse;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Inventory;

public class PartnerInventoryComponent : IPartnerInventoryComponent
{
    private const int WEAR_SLOTS = 13;
    private const byte PARTNER_SLOTS = 56;

    private readonly IPlayerEntity _character;
    private readonly Dictionary<short, PartnerInventoryItem[]> _partnersInventory = new();
    private readonly PartnerWarehouseItem[] _warehouseItems = new PartnerWarehouseItem[PARTNER_SLOTS];
    private static GeneralServerConfiguration GeneralServerConfiguration => StaticGeneralServerConfiguration.Instance;
    public PartnerInventoryComponent(IPlayerEntity character) => _character = character;

    public IReadOnlyList<PartnerInventoryItem> PartnerGetEquippedItems(short partnerSlot) =>
        !_partnersInventory.TryGetValue(partnerSlot, out PartnerInventoryItem[] items) ? Array.Empty<PartnerInventoryItem>() : items;

    public IReadOnlyList<PartnerInventoryItem> GetPartnersEquippedItems()
    {
        return _partnersInventory.Values.SelectMany(s => s).Where(item => item != null).ToList();
    }

    public void PartnerEquipItem(InventoryItem item, short partnerSlot)
    {
        if (item == null)
        {
            return;
        }

        EquipmentType type = item.ItemInstance.GameItem.EquipmentSlot;

        if (type == EquipmentType.SecondaryWeapon)
        {
            type = EquipmentType.MainWeapon;
        }

        var partnerItem = new PartnerInventoryItem
        {
            ItemInstance = item.ItemInstance,
            PartnerSlot = partnerSlot
        };

        if (!_partnersInventory.TryGetValue(partnerSlot, out PartnerInventoryItem[] items))
        {
            items = new PartnerInventoryItem[WEAR_SLOTS];
            _partnersInventory[partnerSlot] = items;
        }

        items[(byte)type] = partnerItem;
    }

    public void PartnerEquipItem(GameItemInstance item, short partnerSlot)
    {
        if (item == null)
        {
            return;
        }

        EquipmentType type = item.GameItem.EquipmentSlot;

        if (type == EquipmentType.SecondaryWeapon)
        {
            type = EquipmentType.MainWeapon;
        }

        var partnerItem = new PartnerInventoryItem
        {
            ItemInstance = item,
            PartnerSlot = partnerSlot
        };

        if (!_partnersInventory.TryGetValue(partnerSlot, out PartnerInventoryItem[] items))
        {
            items = new PartnerInventoryItem[WEAR_SLOTS];
            _partnersInventory[partnerSlot] = items;
        }

        items[(byte)type] = partnerItem;
    }

    public void PartnerTakeOffItem(EquipmentType type, short partnerSlot)
    {
        if (!_partnersInventory.TryGetValue(partnerSlot, out PartnerInventoryItem[] items))
        {
            items = new PartnerInventoryItem[WEAR_SLOTS];
            _partnersInventory[partnerSlot] = items;
        }

        items[(byte)type] = null;
    }

    public PartnerInventoryItem PartnerGetEquippedItem(EquipmentType type, short partnerSlot)
    {
        if (!_partnersInventory.TryGetValue(partnerSlot, out PartnerInventoryItem[] items))
        {
            items = new PartnerInventoryItem[WEAR_SLOTS];
            _partnersInventory[partnerSlot] = items;
        }

        PartnerInventoryItem item = items[(byte)type];
        return item;
    }

    public void AddPartnerWarehouseItem(GameItemInstance item, short slot)
    {
        var newWarehouseItem = new PartnerWarehouseItem
        {
            ItemInstance = item,
            Slot = slot
        };

        _warehouseItems[newWarehouseItem.Slot] = newWarehouseItem;
    }

    public void RemovePartnerWarehouseItem(short slot)
    {
        PartnerWarehouseItem partnerWarehouseItem = GetPartnerWarehouseItem(slot);
        if (partnerWarehouseItem == null)
        {
            return;
        }

        _warehouseItems[partnerWarehouseItem.Slot] = null;
    }

    public PartnerWarehouseItem GetPartnerWarehouseItem(short slot) => _warehouseItems[slot];

    public IReadOnlyList<PartnerWarehouseItem> PartnerWarehouseItems() => _warehouseItems;
    public byte GetPartnerWarehouseSlots() => _character.HaveStaticBonus(StaticBonusType.PartnerBackpack) ? PARTNER_SLOTS : (byte)0;
    public byte GetPartnerWarehouseSlotsWithoutBackpack() => PARTNER_SLOTS;

    public bool HasSpaceForPartnerWarehouseItem() => _warehouseItems.Count(x => x != null) < PARTNER_SLOTS;

    public bool HasSpaceForPartnerItemWarehouse(int itemVnum, int amount = 1)
    {
        if (!_character.HaveStaticBonus(StaticBonusType.PartnerBackpack))
        {
            return false;
        }

        if (!_warehouseItems.Any())
        {
            return true;
        }

        PartnerWarehouseItem[] items = _warehouseItems.OrderBy(x => x?.Slot).ToArray();
        for (byte i = 0; i < PARTNER_SLOTS; i++)
        {
            PartnerWarehouseItem freeSlot = items.FirstOrDefault(x => x != null && x.Slot == i);
            if (freeSlot == null)
            {
                return true;
            }

            if (freeSlot.ItemInstance == null)
            {
                continue;
            }

            if (freeSlot.ItemInstance.GameItem.Id != itemVnum)
            {
                continue;
            }

            if (freeSlot.ItemInstance.GameItem.IsNotStackableInventoryType() || freeSlot.ItemInstance.Amount + amount > GeneralServerConfiguration.MaxItemAmount)
            {
                continue;
            }

            return true;
        }

        return HasSpaceForPartnerWarehouseItem();
    }
}