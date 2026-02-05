using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.DAL;

namespace WingsEmu.DTOs.Mails;

public interface IAccountMailDao : IGenericAsyncLongRepository<AccountMailDto>
{
    Task<List<AccountMailDto>> GetByAccountIdAsync(long accountId);
}