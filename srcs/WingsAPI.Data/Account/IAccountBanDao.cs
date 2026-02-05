using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.DAL;

namespace WingsAPI.Data.Account;

public interface IAccountBanDao : IGenericAsyncLongRepository<AccountBanDto>
{
    Task<AccountBanDto> FindAccountBan(long accountId);
    Task<IEnumerable<AccountBanDto>> GetAccountBans(long accountId);
}