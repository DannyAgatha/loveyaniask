// NosEmu
// 


using System.Collections.Generic;
using WingsEmu.Game.Items;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Inventory;

public interface IInventoryComponent
{
    public IEnumerable<InventoryItem> EquippedItems { get; }

    public GameItemInstance MainWeapon { get; }
    public GameItemInstance SecondaryWeapon { get; }
    public GameItemInstance Armor { get; }

    public GameItemInstance Amulet { get; }
    public GameItemInstance Hat { get; }
    public GameItemInstance Gloves { get; }
    public GameItemInstance Ring { get; }
    public GameItemInstance Necklace { get; }
    public GameItemInstance Bracelet { get; }
    public GameItemInstance Boots { get; }

    public GameItemInstance Fairy { get; }
    public GameItemInstance Specialist { get; }

    public GameItemInstance Mask { get; }
    public GameItemInstance CostumeSuit { get; }
    public GameItemInstance CostumeHat { get; }
    public GameItemInstance WeaponSkin { get; }
    public GameItemInstance Wings { get; }
    public GameItemInstance MiniPet { get; }

    public bool InventoryIsInitialized { get; }
    public IEnumerable<InventoryItem> GetAllPlayerInventoryItems();
    public InventoryItem GetInventoryItemFromEquipmentSlot(EquipmentType type);
    public InventoryItem GetItemBySlotAndType(short slot, InventoryType type);
    public InventoryItem GetFirstItemByVnum(int vnum);
    public InventoryItem FindItemWithoutFullStack(int vnum, int amount);
    public IEnumerable<InventoryItem> GetItemsByInventoryType(InventoryType type);
    public GameItemInstance GetItemInstanceFromEquipmentSlot(EquipmentType type);
    public int CountItemWithVnum(int vnum);
    public bool HasSpaceFor(int vnum, int amount = 1);
    public bool HasItem(int vnum, int amount = 1);
    public void AddItemToInventory(InventoryItem inventoryItem);

    /// <summary>
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="type"></param>
    /// <param name="removedItem"></param>
    /// <returns>false if failed, true if succeeded</returns>
    public bool RemoveItemFromSlotAndType(short slot, InventoryType type, out InventoryItem removedItem);

    public bool RemoveItemAmountByVnum(int vnum, int amount, out InventoryItem removedItem);

    public void EquipItem(InventoryItem item, EquipmentType type, bool force = false);
    public void TakeOffItem(EquipmentType type, short? slot = null, InventoryType? inventoryType = null);
}