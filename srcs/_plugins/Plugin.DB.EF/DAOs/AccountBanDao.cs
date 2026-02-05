using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PhoenixLib.DAL;
using PhoenixLib.Logging;
using Plugin.Database.DB;
using Plugin.Database.Entities.Account;
using WingsAPI.Data.Account;

namespace Plugin.Database.DAOs
{
    public class AccountBanDao : IAccountBanDao
    {
        private readonly IDbContextFactory<GameContext> _contextFactory;
        private readonly IMapper<AccountBanEntity, AccountBanDto> _mapper;
        private readonly IGenericAsyncLongRepository<AccountBanDto> _repository;

        public AccountBanDao(IMapper<AccountBanEntity, AccountBanDto> mapper, IDbContextFactory<GameContext> contextFactory, IGenericAsyncLongRepository<AccountBanDto> repository)
        {
            _mapper = mapper;
            _contextFactory = contextFactory;
            _repository = repository;
        }

        public async Task<AccountBanDto> FindAccountBan(long accountId)
        {
            try
            {
                await using GameContext context = _contextFactory.CreateDbContext();
                AccountBanEntity accountBanEntity = await context.AccountBans.FirstOrDefaultAsync(x => x.AccountId == accountId && (x.End == null || x.End > DateTime.UtcNow));
                return _mapper.Map(accountBanEntity);
            }
            catch (Exception e)
            {
                Log.Error($"FindAccountBan - AccountId: {accountId}", e);
                return null;
            }
        }

        public async Task<IEnumerable<AccountBanDto>> GetAccountBans(long accountId)
        {
            try
            {
                await using GameContext context = _contextFactory.CreateDbContext();
                List<AccountBanEntity> accountBanEntity = await context.AccountBans.Where(x => x.AccountId == accountId).ToListAsync();
                return _mapper.Map(accountBanEntity);
            }
            catch (Exception e)
            {
                Log.Error($"GetAccountBans - AccountId: {accountId}", e);
                return null;
            }
        }

        public async Task<IEnumerable<AccountBanDto>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<AccountBanDto> GetByIdAsync(long id) => await _repository.GetByIdAsync(id);

        public async Task<IEnumerable<AccountBanDto>> GetByIdsAsync(IEnumerable<long> ids) => await _repository.GetByIdsAsync(ids);

        public async Task<AccountBanDto> SaveAsync(AccountBanDto obj) => await _repository.SaveAsync(obj);

        public async Task<IEnumerable<AccountBanDto>> SaveAsync(IReadOnlyList<AccountBanDto> objs) => await _repository.SaveAsync(objs);

        public async Task DeleteByIdAsync(long id) => await _repository.DeleteByIdAsync(id);

        public async Task DeleteByIdsAsync(IEnumerable<long> ids) => await _repository.DeleteByIdsAsync(ids);
    }
}