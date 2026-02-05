using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PhoenixLib.DAL;
using PhoenixLib.Logging;
using Plugin.Database.DB;
using WingsAPI.Data.Families;

namespace Plugin.Database.Families
{
    public class FamilyWarehouseItemDao : IFamilyWarehouseItemDao
    {
        private readonly IDbContextFactory<GameContext> _contextFactory;
        private readonly IMapper<FamilyWarehouseItemEntity, FamilyWarehouseItemDto> _mapper;

        public FamilyWarehouseItemDao(IDbContextFactory<GameContext> contextFactory, IMapper<FamilyWarehouseItemEntity, FamilyWarehouseItemDto> mapper)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
        }

        public async Task<int> SaveAsync(IReadOnlyList<FamilyWarehouseItemDto> objs)
        {
            try
            {
                IEnumerable<FamilyWarehouseItemEntity> entities = _mapper.Map(objs);
                await using GameContext context = _contextFactory.CreateDbContext();
                await context.FamilyWarehouseItems.BulkMergeAsync(entities);
                return await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Log.Error("[FAMILY_WAREHOUSE_ITEM_DAO][SaveAsync] ", e);
                throw;
            }
        }

        public async Task<int> DeleteAsync(IEnumerable<FamilyWarehouseItemDto> objs)
        {
            try
            {
                IEnumerable<FamilyWarehouseItemEntity> entities = _mapper.Map(objs);
                await using GameContext context = _contextFactory.CreateDbContext();
                await context.FamilyWarehouseItems.BulkDeleteAsync(entities);
                return await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Log.Error("[FAMILY_WAREHOUSE_ITEM_DAO][DeleteAsync] ", e);
                throw;
            }
        }

        public async Task<IEnumerable<FamilyWarehouseItemDto>> GetByFamilyIdAsync(long familyId)
        {
            try
            {
                await using GameContext context = _contextFactory.CreateDbContext();
                IEnumerable<FamilyWarehouseItemEntity> items = await context.FamilyWarehouseItems.Where(s => s.FamilyId == familyId).ToListAsync();
                return _mapper.Map(items);
            }
            catch (Exception e)
            {
                Log.Error("[FAMILY_WAREHOUSE_ITEM_DAO][GetByFamilyIdAsync] ", e);
                throw;
            }
        }
    }
}