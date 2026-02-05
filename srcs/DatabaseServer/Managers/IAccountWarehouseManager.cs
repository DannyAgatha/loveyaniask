using System.Collections.Generic;
using System.Threading.Tasks;
using WingsAPI.Data.Account;

namespace DatabaseServer.Managers
{
    public interface IAccountWarehouseManager
    {
        public Task<IEnumerable<AccountWarehouseItemDto>> GetWarehouse(long accountId);
        public Task<AccountWarehouseItemDto> GetWarehouseItem(long accountId, short slot);
        public Task<AddWarehouseItemResult> AddWarehouseItem(AccountWarehouseItemDto warehouseItemDtoToAdd);
        public Task<WithdrawWarehouseItemResult> WithdrawWarehouseItem(AccountWarehouseItemDto warehouseItemDtoToWithdraw, int amount);
        public Task<MoveWarehouseItemResult> MoveWarehouseItem(AccountWarehouseItemDto warehouseItemDtoToMove, int amount, short newSlot);
        public Task FlushWarehouseSaves();
    }
}