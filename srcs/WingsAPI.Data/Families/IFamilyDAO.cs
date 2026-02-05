// NosEmu
// 


using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.DAL;
using WingsAPI.Data.Character;

namespace WingsAPI.Data.Families;

public interface IFamilyDAO : IGenericAsyncLongRepository<FamilyDTO>
{
    Task<FamilyDTO> GetByNameAsync(string reqName);
    Task<long> GetReputationInTotalByIdAsync(long famId);
}