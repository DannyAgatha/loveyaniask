using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PhoenixLib.DAL;
using PhoenixLib.Logging;
using Plugin.Database.DB;
using WingsAPI.Data.Account;
using WingsAPI.Data.Warehouse;

namespace Plugin.Database.Warehouse
{
    public class AccountWarehouseItemItemDao : IAccountWarehouseItemDao
    {
        private readonly IDbContextFactory<GameContext> _contextFactory;
        private readonly IMapper<AccountWarehouseItemEntity, AccountWarehouseItemDto> _mapper;

        public AccountWarehouseItemItemDao(IDbContextFactory<GameContext> contextFactory, IMapper<AccountWarehouseItemEntity, AccountWarehouseItemDto> mapper)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
        }

        public async Task<int> SaveAsync(IReadOnlyList<AccountWarehouseItemDto> objs)
        {
            try
            {
                IEnumerable<AccountWarehouseItemEntity> entities = _mapper.Map(objs);
                await using GameContext context = _contextFactory.CreateDbContext();
                await context.AccountWarehouseItems.BulkMergeAsync(entities);
                return await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Log.Error("[ACCOUNT_WAREHOUSE_ITEM_DAO][SaveAsync] ", e);
                throw;
            }
        }

        public async Task<int> DeleteAsync(IEnumerable<AccountWarehouseItemDto> objs)
        {
            try
            {
                IEnumerable<AccountWarehouseItemEntity> entities = _mapper.Map(objs);
                await using GameContext context = _contextFactory.CreateDbContext();
                await context.AccountWarehouseItems.BulkDeleteAsync(entities);
                return await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Log.Error("[ACCOUNT_WAREHOUSE_ITEM_DAO][DeleteAsync] ", e);
                throw;
            }
        }

        public async Task<IEnumerable<AccountWarehouseItemDto>> GetByAccountIdAsync(long accountId)
        {
            try
            {
                await using GameContext context = _contextFactory.CreateDbContext();
                IEnumerable<AccountWarehouseItemEntity> items = await context.AccountWarehouseItems.Where(s => s.AccountId == accountId).ToListAsync();
                return _mapper.Map(items);
            }
            catch (Exception e)
            {
                Log.Error("[ACCOUNT_WAREHOUSE_ITEM_DAO][GetByFamilyIdAsync] ", e);
                throw;
            }
        }
    }
}