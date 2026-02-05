using System.Collections.Generic;
using System.Threading.Tasks;
using WingsAPI.Data.Families;

namespace FamilyServer.Managers
{
    public interface IFamilyWarehouseManager
    {
        public Task<IList<FamilyWarehouseLogEntryDto>> GetWarehouseLogs(long familyId, long? characterId = null);
        public Task<IEnumerable<FamilyWarehouseItemDto>> GetWarehouse(long familyId, long? characterId = null);
        public Task<FamilyWarehouseItemDto> GetWarehouseItem(long familyId, short slot, long? characterId = null);
        public Task<AddWarehouseItemResult> AddWarehouseItem(FamilyWarehouseItemDto warehouseItemDtoToAdd, long? characterId = null, string characterName = null);
        public Task<WithdrawWarehouseItemResult> WithdrawWarehouseItem(FamilyWarehouseItemDto warehouseItemDtoToWithdraw, int amount, long? characterId = null, string characterName = null);
        public Task<MoveWarehouseItemResult> MoveWarehouseItem(FamilyWarehouseItemDto warehouseItemDtoToMove, int amount, short newSlot, long? characterId = null);
        public Task FlushWarehouseSaves();
    }
}