// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PhoenixLib.DAL.EFCore.PGSQL
{
    /// <summary>
    ///     GenericAsyncMappedRepository is an asynchronous Data Access Object
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TDto"></typeparam>
    /// <typeparam name="TDbContext"></typeparam>
    [Obsolete("Use GenericMappedUuidRepository now")]
    public class GenericOldMappedUuidRepository<TEntity, TDto, TDbContext> : IGenericAsyncUuidRepository<TDto>
    where TDto : class, IUuidDto, new()
    where TEntity : class, IUuidEntity, new()
    where TDbContext : DbContext
    {
        private readonly IDbContextFactory<TDbContext> _contextFactory;
        private readonly ILogger<GenericOldMappedUuidRepository<TEntity, TDto, TDbContext>> _logger;
        private readonly IMapper<TEntity, TDto> _mapper;

        public GenericOldMappedUuidRepository(IDbContextFactory<TDbContext> contextFactory, IMapper<TEntity, TDto> mapper, ILogger<GenericOldMappedUuidRepository<TEntity, TDto, TDbContext>> logger)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<TDto>> GetAllAsync()
        {
            try
            {
                await using DbContext context = _contextFactory.CreateDbContext();
                List<TEntity> tmp = await context.Set<TEntity>().ToListAsync();
                return _mapper.Map(tmp);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetAllAsync");
                throw;
            }
        }


        public async Task<TDto> GetByIdAsync(Guid id)
        {
            try
            {
                await using DbContext context = _contextFactory.CreateDbContext();
                TEntity tmp = await context.Set<TEntity>().FirstOrDefaultAsync(s => s.Id == id);
                return _mapper.Map(tmp);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetByIdAsync");
                throw;
            }
        }

        public async Task<IEnumerable<TDto>> GetByIdsAsync(IEnumerable<Guid> ids)
        {
            try
            {
                await using DbContext context = _contextFactory.CreateDbContext();
                List<TEntity> tmp = await context.Set<TEntity>().Where(s => ids.Contains(s.Id)).ToListAsync();
                return _mapper.Map(tmp);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetByIdsAsync");
                throw;
            }
        }

        public async Task<TDto> SaveAsync(TDto obj)
        {
            try
            {
                await using DbContext context = _contextFactory.CreateDbContext();
                TEntity entity = _mapper.Map(obj);
                await context.SingleMergeAsync(obj, operation =>
                {
                    operation.InsertKeepIdentity = true;
                    operation.IsCheckConstraintOnInsertDisabled = false;
                });
                return _mapper.Map(entity);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "SaveAsync");
                throw;
            }
        }

        public async Task<IEnumerable<TDto>> SaveAsync(IReadOnlyList<TDto> objs)
        {
            try
            {
                var entities = new List<TEntity>(_mapper.Map(objs));
                await using DbContext context = _contextFactory.CreateDbContext();
                await context.BulkMergeAsync(entities, operation =>
                {
                    operation.InsertKeepIdentity = true;
                    operation.IsCheckConstraintOnInsertDisabled = false;
                });
                return _mapper.Map(entities);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"SaveAsync<{typeof(TEntity).Name}>");
                throw;
            }
        }

        public async Task DeleteByIdAsync(Guid id)
        {
            try
            {
                await using DbContext context = _contextFactory.CreateDbContext();
                TEntity entity = await context.FindAsync<TEntity>(id);
                if (entity == null)
                {
                    return;
                }

                context.Set<TEntity>().Remove(entity);
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "DeleteByIdAsync");
                throw;
            }
        }

        public async Task DeleteByIdsAsync(IEnumerable<Guid> ids)
        {
            try
            {
                await using DbContext context = _contextFactory.CreateDbContext();
                foreach (Guid id in ids)
                {
                    TEntity entity = await context.FindAsync<TEntity>(id);
                    if (entity == null)
                    {
                        continue;
                    }

                    context.Set<TEntity>().Remove(entity);
                }

                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "DeleteByIdsAsync");
                throw;
            }
        }
    }
}