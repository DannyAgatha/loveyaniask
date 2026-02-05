// NosEmu
// 


using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.DAL;

namespace WingsAPI.Data.Families;

public interface IFamilyMembershipDao : IGenericAsyncLongRepository<FamilyMembershipDto>
{
    Task<FamilyMembershipDto> GetByCharacterIdAsync(long characterId);

    Task<List<FamilyMembershipDto>> GetByFamilyIdAsync(long familyId);
}