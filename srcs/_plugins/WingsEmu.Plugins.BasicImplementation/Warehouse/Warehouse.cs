using System.Collections.Generic;
using System.Linq;
using WingsEmu.Game.Characters;

namespace WingsEmu.Game.Warehouse;

public class Warehouse : IWarehouse
{
    private const byte WARHOUSE_SLOTS = 98;

    private readonly IPlayerEntity _playerEntity;
    private readonly WarehouseItem[] _warehouseItems = new WarehouseItem[WARHOUSE_SLOTS];

    public Warehouse(IPlayerEntity playerEntity) => _playerEntity = playerEntity;

    public void AddWarehouseItem(WarehouseItem item, bool force = false)
    {
        if (_playerEntity.WareHouseSize <= 0 && !force)
        {
            return;
        }

        _warehouseItems[item.Slot] = item;
    }

    public void RemoveWarehouseItem(short slot)
    {
        WarehouseItem partnerWarehouseItem = GetWarehouseItem(slot);
        if (partnerWarehouseItem == null)
        {
            return;
        }

        _warehouseItems[partnerWarehouseItem.Slot] = null;
    }

    public WarehouseItem GetWarehouseItem(short slot) => _warehouseItems[slot];

    public IReadOnlyList<WarehouseItem> WarehouseItems() => _warehouseItems;
    public int GetWarehouseSlots() => _playerEntity.WareHouseSize;
    public bool HasSpaceForWarehouseItem() => _warehouseItems.Count(x => x != null) <= WARHOUSE_SLOTS;
}