using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WingsEmu.Core.Extensions;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Inventory;

public class InventoryComponent : IInventoryComponent
{
    private const int WEAR_SLOTS = 18;
    private readonly IPlayerEntity _character;
    private readonly Dictionary<InventoryType, List<InventoryItem>> _inventoryItemsByInventoryType = new();
    private readonly IItemsManager _itemsManager;
    private readonly ReaderWriterLockSlim _lock = new();

    private readonly InventoryItem[] _wear = new InventoryItem[WEAR_SLOTS];
    private static GeneralServerConfiguration GeneralServerConfiguration => StaticGeneralServerConfiguration.Instance;

    public InventoryComponent(IPlayerEntity character, IItemsManager itemsManager)
    {
        _character = character;
        _itemsManager = itemsManager;
    }

    public IEnumerable<InventoryItem> GetAllPlayerInventoryItems()
    {
        var list = new List<InventoryItem>();
        _lock.EnterReadLock();
        try
        {
            foreach (KeyValuePair<InventoryType, List<InventoryItem>> keyValuePair in _inventoryItemsByInventoryType)
            {
                list.AddRange(keyValuePair.Value);
            }

            foreach (InventoryItem inventoryItem in _wear)
            {
                if (inventoryItem == null)
                {
                    continue;
                }

                list.Add(inventoryItem);
            }

            return list;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public InventoryItem GetFirstItemByVnum(int vnum)
    {
        IGameItem gameItem = _itemsManager.GetItem(vnum);
        if (gameItem == null)
        {
            return null;
        }

        _lock.EnterReadLock();
        try
        {
            if (!_inventoryItemsByInventoryType.TryGetValue(gameItem.Type, out List<InventoryItem> items))
            {
                return null;
            }

            return items.Where(s => s?.ItemInstance != null && s.ItemInstance.ItemVNum == vnum).OrderBy(x => x.Slot).FirstOrDefault();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public InventoryItem FindItemWithoutFullStack(int vnum, int amount)
    {
        IGameItem gameItem = _itemsManager.GetItem(vnum);
        if (gameItem == null)
        {
            return null;
        }

        _lock.EnterReadLock();
        try
        {
            if (!_inventoryItemsByInventoryType.TryGetValue(gameItem.Type, out List<InventoryItem> items))
            {
                return null;
            }

            return items.Where(s => s?.ItemInstance != null && s.ItemInstance.ItemVNum == vnum && s.ItemInstance.Amount + amount <= GeneralServerConfiguration.MaxItemAmount).OrderBy(x => x.Slot).FirstOrDefault();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public IEnumerable<InventoryItem> GetItemsByInventoryType(InventoryType type)
    {
        if (type == InventoryType.EquippedItems)
        {
            return Array.Empty<InventoryItem>();
        }

        _lock.EnterReadLock();
        try
        {
            if (_inventoryItemsByInventoryType.TryGetValue(type, out List<InventoryItem> items))
            {
                return items;
            }

            return Array.Empty<InventoryItem>();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public IEnumerable<InventoryItem> EquippedItems => _wear;
    public GameItemInstance GetItemInstanceFromEquipmentSlot(EquipmentType type) => _wear[(byte)type]?.ItemInstance;

    public GameItemInstance MainWeapon => _wear[(byte)EquipmentType.MainWeapon]?.ItemInstance;
    public GameItemInstance SecondaryWeapon => _wear[(byte)EquipmentType.SecondaryWeapon]?.ItemInstance;
    public GameItemInstance Armor => _wear[(byte)EquipmentType.Armor]?.ItemInstance;
    public GameItemInstance Hat => _wear[(byte)EquipmentType.Hat]?.ItemInstance;
    public GameItemInstance Amulet => _wear[(byte)EquipmentType.Amulet]?.ItemInstance;
    public GameItemInstance Gloves => _wear[(byte)EquipmentType.Gloves]?.ItemInstance;
    public GameItemInstance Ring => _wear[(byte)EquipmentType.Ring]?.ItemInstance;
    public GameItemInstance Necklace => _wear[(byte)EquipmentType.Necklace]?.ItemInstance;
    public GameItemInstance Bracelet => _wear[(byte)EquipmentType.Bracelet]?.ItemInstance;
    public GameItemInstance Boots => _wear[(byte)EquipmentType.Boots]?.ItemInstance;
    public GameItemInstance Fairy => _wear[(byte)EquipmentType.Fairy]?.ItemInstance;
    public GameItemInstance Mask => _wear[(byte)EquipmentType.Mask]?.ItemInstance;
    public GameItemInstance CostumeSuit => _wear[(byte)EquipmentType.CostumeSuit]?.ItemInstance;
    public GameItemInstance CostumeHat => _wear[(byte)EquipmentType.CostumeHat]?.ItemInstance;
    public GameItemInstance WeaponSkin => _wear[(byte)EquipmentType.WeaponSkin]?.ItemInstance;
    public GameItemInstance Wings => _wear[(byte)EquipmentType.Wings]?.ItemInstance;
    public GameItemInstance MiniPet => _wear[(byte)EquipmentType.MiniPet]?.ItemInstance;
    public GameItemInstance Specialist => _wear[(byte)EquipmentType.Sp]?.ItemInstance;

    public bool InventoryIsInitialized { get; } = false;

    public int CountItemWithVnum(int vnum)
    {
        IGameItem gameItem = _itemsManager.GetItem(vnum);
        if (gameItem == null)
        {
            return default;
        }

        _lock.EnterReadLock();
        try
        {
            if (!_inventoryItemsByInventoryType.TryGetValue(gameItem.Type, out List<InventoryItem> items))
            {
                return 0;
            }

            return items.Where(s => s?.ItemInstance != null && s.ItemInstance.ItemVNum == vnum).Sum(s => s.ItemInstance.Amount);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool HasSpaceFor(int vnum, int amount = 1)
    {
        IGameItem gameItem = _itemsManager.GetItem(vnum);
        if (gameItem == null)
        {
            return false;
        }

        _lock.EnterReadLock();
        try
        {
            if (!_inventoryItemsByInventoryType.TryGetValue(gameItem.Type, out List<InventoryItem> items))
            {
                // no inventory in that InventoryType
                return true;
            }

            if (items == null || !items.Any())
            {
                return true;
            }

            // Find first free inventory slot - previous version was looking for by lists index, but now is by item slot
            short slots = _character.GetInventorySlots(false, gameItem.Type);

            for (short i = 0; i < slots; i++)
            {
                InventoryItem freeSlot = items.FirstOrDefault(x => x != null && x.Slot == i);
                if (freeSlot == null)
                {
                    return true;
                }

                if (freeSlot.ItemInstance == null)
                {
                    continue;
                }

                if (freeSlot.ItemInstance.GameItem.Id != vnum)
                {
                    continue;
                }

                if (freeSlot.ItemInstance.GameItem.IsNotStackableInventoryType() || freeSlot.ItemInstance.Amount + amount > GeneralServerConfiguration.MaxItemAmount)
                {
                    continue;
                }

                return true;
            }

            return items.Count(s => s?.ItemInstance != null) <= _character.GetInventorySlots(true, gameItem.Type);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool HasItem(int vnum, int amount = 1)
    {
        IGameItem gameItem = _itemsManager.GetItem(vnum);
        if (gameItem == null)
        {
            return false;
        }

        _lock.EnterReadLock();
        try
        {
            if (!_inventoryItemsByInventoryType.TryGetValue(gameItem.Type, out List<InventoryItem> items))
            {
                return false;
            }

            return items.Where(s => s?.ItemInstance != null && s.ItemInstance.ItemVNum == vnum).Sum(s => s.ItemInstance.Amount) >= amount;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }


    public bool RemoveItemAmountByVnum(int vnum, int amount, out InventoryItem removedItem)
    {
        IGameItem gameItem = _itemsManager.GetItem(vnum);
        if (gameItem == null)
        {
            removedItem = null;
            return false;
        }

        _lock.EnterWriteLock();
        try
        {
            InventoryItem getItem = GetFirstItemByVnum(vnum);
            if (getItem == null)
            {
                removedItem = null;
                return false;
            }

            getItem.ItemInstance.Amount -= amount;
            removedItem = getItem;
            return true;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void EquipItem(InventoryItem item, EquipmentType type, bool force = false)
    {
        if (force == false)
        {
            RemoveItemFromSlotAndType(item.Slot, item.InventoryType, out InventoryItem _);
        }

        item.Slot = (short)item.ItemInstance.GameItem.EquipmentSlot;
        item.InventoryType = InventoryType.EquippedItems;
        item.IsEquipped = true;
        _wear[(byte)type] = item;
    }

    public void TakeOffItem(EquipmentType type, short? slot = null, InventoryType? inventoryType = null)
    {
        InventoryItem item = _wear[(byte)type];
        if (item == null)
        {
            return;
        }

        InventoryType invType = inventoryType ?? InventoryType.Equipment;
        item.InventoryType = invType;
        item.IsEquipped = false;
        item.Slot = slot ?? _character.GetNextInventorySlot(invType);
        AddItemToInventory(item);
        _wear[(byte)type] = null;
    }

    public void AddItemToInventory(InventoryItem inventoryItem)
    {
        InventoryType inventoryType = inventoryItem.InventoryType;
        if (inventoryType == InventoryType.EquippedItems)
        {
            return;
        }

        _lock.EnterReadLock();
        try
        {
            if (!_inventoryItemsByInventoryType.TryGetValue(inventoryType, out List<InventoryItem> item))
            {
                item = new List<InventoryItem>();
                _inventoryItemsByInventoryType[inventoryType] = item;
            }

            item.Add(inventoryItem);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool RemoveItemFromSlotAndType(short slot, InventoryType type, out InventoryItem removedItem)
    {
        if (type == InventoryType.EquippedItems)
        {
            removedItem = null;
            return false;
        }

        _lock.EnterReadLock();
        try
        {
            if (!_inventoryItemsByInventoryType.TryGetValue(type, out List<InventoryItem> inventoryItems))
            {
                removedItem = null;
                return false;
            }

            InventoryItem item = inventoryItems.FirstOrDefault(x => x?.Slot == slot);
            if (item == null)
            {
                removedItem = null;
                return false;
            }

            List<InventoryItem> items = _inventoryItemsByInventoryType.GetOrDefault(type);
            if (items == null)
            {
                removedItem = null;
                return false;
            }

            items.Remove(item);
            removedItem = item;
            return true;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public InventoryItem GetInventoryItemFromEquipmentSlot(EquipmentType type) => _wear[(byte)type] == null ? null : _wear[(byte)type];

    public InventoryItem GetItemBySlotAndType(short slot, InventoryType type)
    {
        if (type == InventoryType.EquippedItems)
        {
            return null;
        }

        _lock.EnterReadLock();
        try
        {
            if (!_inventoryItemsByInventoryType.TryGetValue(type, out List<InventoryItem> item))
            {
                return null;
            }

            return item.FirstOrDefault(x => x?.Slot == slot);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public InventoryItem GetItemBySlotAndType(short slot, InventoryType type, bool force)
    {
        if (type == InventoryType.EquippedItems && force == false)
        {
            return null;
        }

        _lock.EnterReadLock();
        try
        {
            if (!_inventoryItemsByInventoryType.TryGetValue(type, out List<InventoryItem> item))
            {
                return null;
            }

            return item.FirstOrDefault(x => x?.Slot == slot);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
}