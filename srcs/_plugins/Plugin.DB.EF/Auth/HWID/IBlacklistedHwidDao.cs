using System.Collections.Generic;
using System.Threading.Tasks;
using WingsAPI.Communication.Auth;

namespace Plugin.Database.Auth.HWID
{
    public interface IBlacklistedHwidDao
    {
        Task SaveAsync(BlacklistedHwidDto dto);
        Task DeleteAsync(string hwid);
        Task<BlacklistedHwidDto> GetByKeyAsync(string hwid);
        Task<IEnumerable<BlacklistedHwidDto>> GetAllAsync();
    }
}