using System.Collections.Generic;
using WingsEmu.Game.Items;
using WingsEmu.Game.Warehouse;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Inventory;

public interface IPartnerInventoryComponent
{
    public IReadOnlyList<PartnerInventoryItem> PartnerGetEquippedItems(short partnerSlot);
    public IReadOnlyList<PartnerInventoryItem> GetPartnersEquippedItems();
    public void PartnerEquipItem(InventoryItem item, short partnerSlot);
    public void PartnerEquipItem(GameItemInstance item, short partnerSlot);
    public void PartnerTakeOffItem(EquipmentType type, short partnerSlot);
    public PartnerInventoryItem PartnerGetEquippedItem(EquipmentType type, short partnerSlot);

    public void AddPartnerWarehouseItem(GameItemInstance item, short slot);
    public void RemovePartnerWarehouseItem(short slot);
    public PartnerWarehouseItem GetPartnerWarehouseItem(short slot);
    public IReadOnlyList<PartnerWarehouseItem> PartnerWarehouseItems();
    public byte GetPartnerWarehouseSlots();
    public byte GetPartnerWarehouseSlotsWithoutBackpack();
    public bool HasSpaceForPartnerWarehouseItem();
    public bool HasSpaceForPartnerItemWarehouse(int itemVnum, int amount = 1);
}