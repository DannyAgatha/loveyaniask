using System.Collections.Generic;
using System.Threading.Tasks;

namespace WingsAPI.Data.Families;

public interface IFamilyWarehouseLogDao
{
    Task<int> SaveAsync(long familyId, IEnumerable<FamilyWarehouseLogEntryDto> objs);
    Task<IEnumerable<FamilyWarehouseLogEntryDto>> GetByFamilyIdAsync(long familyId);
}