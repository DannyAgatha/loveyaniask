// NosEmu
// 


using System.Collections.Generic;
using System.Threading.Tasks;
using WingsAPI.Data.Account;

namespace WingsAPI.Data.Warehouse;

public interface IAccountWarehouseItemDao
{
    Task<int> SaveAsync(IReadOnlyList<AccountWarehouseItemDto> objs);
    Task<int> DeleteAsync(IEnumerable<AccountWarehouseItemDto> objs);
    Task<IEnumerable<AccountWarehouseItemDto>> GetByAccountIdAsync(long accountId);
}