using System.Collections.Generic;
using System.Threading.Tasks;
using WingsAPI.Data.Families;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._enum;

namespace Plugin.FamilyImpl
{
    public interface IFamilyWarehouseManager
    {
        public Task<(IList<FamilyWarehouseLogEntryDto>, ManagerResponseType?)> GetWarehouseLogs(long familyId, long characterId);
        public Task<(IDictionary<short, FamilyWarehouseItemDto> familyWarehouseItemDtos, ManagerResponseType?)> GetWarehouse(long familyId, long characterId);
        public Task<(FamilyWarehouseItemDto, ManagerResponseType?)> GetWarehouseItem(long familyId, short slot, long characterId);
        public Task<ManagerResponseType?> AddWarehouseItem(FamilyWarehouseItemDto warehouseItemDtoToAdd, long characterId, string characterName);
        public Task<(ItemInstanceDTO, ManagerResponseType?)> WithdrawWarehouseItem(FamilyWarehouseItemDto warehouseItemDtoToWithdraw, int amount, long characterId, string characterName);
        public Task<ManagerResponseType?> MoveWarehouseItem(FamilyWarehouseItemDto warehouseItemDtoToMove, int amount, short newSlot, long characterId);
        public Task UpdateWarehouseItem(long familyId, IEnumerable<(FamilyWarehouseItemDto, short)> warehouseItemDtosToUpdate);
        public Task AddWarehouseLog(long familyId, FamilyWarehouseLogEntryDto log);
    }
}