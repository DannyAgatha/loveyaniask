using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PhoenixLib.DAL;
using PhoenixLib.Logging;
using Plugin.Database.DB;
using Plugin.Database.Entities.ServerData;
using WingsAPI.Data.TimeSpace;

namespace Plugin.Database.DAOs
{
    public class TimeSpaceRecordDao : ITimeSpaceRecordDao
    {
        private readonly IDbContextFactory<GameContext> _contextFactory;
        private readonly IMapper<DbTimeSpaceRecord, TimeSpaceRecordDto> _mapper;

        public TimeSpaceRecordDao(IMapper<DbTimeSpaceRecord, TimeSpaceRecordDto> mapper, IDbContextFactory<GameContext> contextFactory)
        {
            _mapper = mapper;
            _contextFactory = contextFactory;
        }

        public async Task<TimeSpaceRecordDto> GetRecordById(long timeSpaceId)
        {
            try
            {
                await using GameContext context = _contextFactory.CreateDbContext();
                DbTimeSpaceRecord entity = await context.Set<DbTimeSpaceRecord>().FindAsync(timeSpaceId);
                return _mapper.Map(entity);
            }
            catch (Exception e)
            {
                Log.Error("GetRecordById", e);
                throw;
            }
        }

        public async Task SaveRecord(TimeSpaceRecordDto recordDto)
        {
            await using GameContext context = _contextFactory.CreateDbContext();
            try
            {
                DbTimeSpaceRecord obj = _mapper.Map(recordDto);
                DbTimeSpaceRecord entity = await context.Set<DbTimeSpaceRecord>().FindAsync(obj.TimeSpaceId);

                if (entity == null)
                {
                    entity = obj;
                    await context.Set<DbTimeSpaceRecord>().AddAsync(entity);
                }
                else
                {
                    context.Update(obj);
                }

                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Log.Error("SaveRecord", e);
                throw;
            }
        }

        public async Task<IEnumerable<TimeSpaceRecordDto>> GetAllRecords()
        {
            await using GameContext context = _contextFactory.CreateDbContext();
            var list = new List<TimeSpaceRecordDto>();

            try
            {
                List<DbTimeSpaceRecord> relations = await context.TimeSpaceRecords.ToListAsync();
                foreach (DbTimeSpaceRecord relation in relations)
                {
                    list.Add(_mapper.Map(relation));
                }

                return list;
            }
            catch (Exception e)
            {
                Log.Error("GetAllRecords", e);
                throw;
            }
        }
    }
}