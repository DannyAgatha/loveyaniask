using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PhoenixLib.DAL;
using Plugin.Database.DB;
using WingsAPI.Communication.Auth;

namespace Plugin.Database.Auth.HWID
{
    public class BlacklistedHwidDao : IBlacklistedHwidDao
    {
        private readonly IDbContextFactory<GameContext> _contextFactory;
        private readonly ILogger<BlacklistedHwidDao> _logger;
        private readonly IMapper<BlacklistedHwidDto, BlacklistedHwidEntity> _mapper;

        public BlacklistedHwidDao(IDbContextFactory<GameContext> contextFactory, IMapper<BlacklistedHwidDto, BlacklistedHwidEntity> mapper, ILogger<BlacklistedHwidDao> logger)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task SaveAsync(BlacklistedHwidDto dto)
        {
            try
            {
                await using DbContext context = _contextFactory.CreateDbContext();

                BlacklistedHwidEntity entity = await context.FindAsync<BlacklistedHwidEntity>(dto.HardwareId);

                if (entity == null)
                {
                    entity = dto.Adapt<BlacklistedHwidEntity>();
                    await context.Set<BlacklistedHwidEntity>().AddAsync(entity);
                }
                else
                {
                    dto.Adapt(entity);
                    context.Set<BlacklistedHwidEntity>().Update(entity);
                }

                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError("[BLACKLISTED_HWID_DAO][SaveAsync] ", e);
                throw;
            }
        }

        public async Task DeleteAsync(string hwid)
        {
            try
            {
                await using GameContext context = _contextFactory.CreateDbContext();
                BlacklistedHwidEntity entity = await context.BlacklistedHwids.FindAsync(hwid);
                context.BlacklistedHwids.Remove(entity);
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError("[BLACKLISTED_HWID_DAO][DeleteAsync] ", e);
                throw;
            }
        }

        public async Task<BlacklistedHwidDto> GetByKeyAsync(string hwid)
        {
            try
            {
                await using GameContext context = _contextFactory.CreateDbContext();
                BlacklistedHwidEntity item = await context.BlacklistedHwids.FindAsync(hwid);
                return item == null ? null : _mapper.Map(item);
            }
            catch (Exception e)
            {
                _logger.LogError("[BLACKLISTED_HWID_DAO][GetByKeyAsync] ", e);
                throw;
            }
        }

        public async Task<IEnumerable<BlacklistedHwidDto>> GetAllAsync()
        {
            try
            {
                await using GameContext context = _contextFactory.CreateDbContext();
                List<BlacklistedHwidEntity> items = await context.BlacklistedHwids.ToListAsync();
                return _mapper.Map(items);
            }
            catch (Exception e)
            {
                _logger.LogError("[BLACKLISTED_HWID_DAO][GetAllAsync] ", e);
                throw;
            }
        }
    }
}