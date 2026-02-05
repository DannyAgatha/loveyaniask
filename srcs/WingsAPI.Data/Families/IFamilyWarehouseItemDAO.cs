using System.Collections.Generic;
using System.Threading.Tasks;

namespace WingsAPI.Data.Families;

public interface IFamilyWarehouseItemDao
{
    Task<int> SaveAsync(IReadOnlyList<FamilyWarehouseItemDto> objs);
    Task<int> DeleteAsync(IEnumerable<FamilyWarehouseItemDto> objs);
    Task<IEnumerable<FamilyWarehouseItemDto>> GetByFamilyIdAsync(long familyId);
}