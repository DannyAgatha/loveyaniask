using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.DAL;

namespace WingsAPI.Data.Account;

public interface IAccountPenaltyDao : IGenericAsyncLongRepository<AccountPenaltyDto>
{
    Task<List<AccountPenaltyDto>> GetPenaltiesByAccountId(long accountId);
}