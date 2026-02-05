using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.DAL;

namespace WingsAPI.Data.Account;

public interface IRefreshTokensDao : IGenericAsyncLongRepository<RefreshTokensDto>
{
    Task<RefreshTokensDto> FindToken(long accountId, string token);
    
    Task<IEnumerable<RefreshTokensDto>> GetAccountTokens(long accountId);
}