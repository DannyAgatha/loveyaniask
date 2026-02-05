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
    public class AccountPenaltyDao : IAccountPenaltyDao
    {
        private readonly IDbContextFactory<GameContext> _contextFactory;
        private readonly IMapper<AccountPenaltyEntity, AccountPenaltyDto> _mapper;
        private readonly IGenericAsyncLongRepository<AccountPenaltyDto> _repository;

        public AccountPenaltyDao(IMapper<AccountPenaltyEntity, AccountPenaltyDto> mapper, IDbContextFactory<GameContext> contextFactory, IGenericAsyncLongRepository<AccountPenaltyDto> repository)
        {
            _mapper = mapper;
            _contextFactory = contextFactory;
            _repository = repository;
        }

        public async Task<List<AccountPenaltyDto>> GetPenaltiesByAccountId(long accountId)
        {
            try
            {
                await using GameContext context = _contextFactory.CreateDbContext();
                List<AccountPenaltyEntity> accountBanEntity = await context.AccountPenalties.Where(x => x.AccountId == accountId).ToListAsync();
                return _mapper.Map(accountBanEntity);
            }
            catch (Exception e)
            {
                Log.Error($"GetPenaltiesByAccountId - AccountId: {accountId}", e);
                return new List<AccountPenaltyDto>();
            }
        }

        public async Task<IEnumerable<AccountPenaltyDto>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<AccountPenaltyDto> GetByIdAsync(long id) => await _repository.GetByIdAsync(id);

        public async Task<IEnumerable<AccountPenaltyDto>> GetByIdsAsync(IEnumerable<long> ids) => await _repository.GetByIdsAsync(ids);

        public async Task<AccountPenaltyDto> SaveAsync(AccountPenaltyDto obj) => await _repository.SaveAsync(obj);

        public async Task<IEnumerable<AccountPenaltyDto>> SaveAsync(IReadOnlyList<AccountPenaltyDto> objs) => await _repository.SaveAsync(objs);

        public async Task DeleteByIdAsync(long id) => await _repository.DeleteByIdAsync(id);

        public async Task DeleteByIdsAsync(IEnumerable<long> ids) => await _repository.DeleteByIdsAsync(ids);
    }
}