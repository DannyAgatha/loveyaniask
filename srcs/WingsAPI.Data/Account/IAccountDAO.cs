// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.DAL;

namespace WingsAPI.Data.Account;

public interface IAccountDAO : IGenericAsyncLongRepository<AccountDTO>
{
    AccountDTO LoadByName(string name);

    Task<AccountDTO> GetByNameAsync(string name);
    Task<List<AccountDTO>> LoadByMasterAccountIdAsync(Guid masterAccountId);
}