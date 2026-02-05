using System.Collections.Generic;

namespace WingsEmu.Game.Warehouse;

public interface IWarehouse
{
    public void AddWarehouseItem(WarehouseItem item, bool force = false);
    public void RemoveWarehouseItem(short slot);
    public WarehouseItem GetWarehouseItem(short slot);
    public IReadOnlyList<WarehouseItem> WarehouseItems();
    public int GetWarehouseSlots();
    public bool HasSpaceForWarehouseItem();
}