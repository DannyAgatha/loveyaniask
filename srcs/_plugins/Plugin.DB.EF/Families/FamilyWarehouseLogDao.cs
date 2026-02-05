using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PhoenixLib.Logging;
using Plugin.Database.DB;
using WingsAPI.Data.Families;

namespace Plugin.Database.Families
{
    public class FamilyWarehouseLogDao : IFamilyWarehouseLogDao
    {
        private readonly IDbContextFactory<GameContext> _contextFactory;

        public FamilyWarehouseLogDao(IDbContextFactory<GameContext> contextFactory) => _contextFactory = contextFactory;

        public async Task<int> SaveAsync(long familyId, IEnumerable<FamilyWarehouseLogEntryDto> objs)
        {
            try
            {
                await using GameContext context = _contextFactory.CreateDbContext();
                await context.FamilyWarehouseLogs.SingleMergeAsync(new FamilyWarehouseLogEntity
                {
                    FamilyId = familyId,
                    LogEntries = objs.ToList()
                });
                return await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Log.Error("[FAMILY_WAREHOUSE_LOG_DAO][SaveAsync] ", e);
                throw;
            }
        }

        public async Task<IEnumerable<FamilyWarehouseLogEntryDto>> GetByFamilyIdAsync(long familyId)
        {
            try
            {
                await using GameContext context = _contextFactory.CreateDbContext();
                FamilyWarehouseLogEntity logs = await context.FamilyWarehouseLogs.FindAsync(familyId);
                return logs?.LogEntries;
            }
            catch (Exception e)
            {
                Log.Error("[FAMILY_WAREHOUSE_LOG_DAO][GetByFamilyIdAsync] ", e);
                throw;
            }
        }
    }
}