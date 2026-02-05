// NosEmu
// 


using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.DAL;

namespace WingsAPI.Data.Families;

public interface IFamilyLogDAO : IGenericAsyncLongRepository<FamilyLogDto>
{
    /// <summary>
    ///     Gets a list of 200 logs ordered by DateTime
    /// </summary>
    /// <param name="familyId"></param>
    /// <returns></returns>
    Task<List<FamilyLogDto>> GetLogsByFamilyIdAsync(long familyId);
}