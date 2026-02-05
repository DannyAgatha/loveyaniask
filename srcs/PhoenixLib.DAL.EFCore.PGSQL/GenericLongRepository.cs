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
    /// <typeparam name="TDbContext"></typeparam>
    public sealed class GenericLongRepository<TEntity, TDbContext> : IGenericAsyncLongRepository<TEntity>
    where TEntity : class, ILongEntity, new()
    where TDbContext : DbContext
    {
        private static readonly string _typeName = typeof(TEntity).Name;
        private readonly IDbContextFactory<TDbContext> _contextFactory;

        private readonly ILogger<GenericLongRepository<TEntity, TDbContext>> _logger;

        public GenericLongRepository(IDbContextFactory<TDbContext> contextFactory, ILogger<GenericLongRepository<TEntity, TDbContext>> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            try
            {
                await using TDbContext context = _contextFactory.CreateDbContext();
                List<TEntity> tmp = await context.Set<TEntity>().ToListAsync();
                return tmp;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"GetAllAsync {_typeName}");
                throw;
            }
        }


        public async Task<TEntity> GetByIdAsync(long id)
        {
            try
            {
                await using DbContext context = _contextFactory.CreateDbContext();
                TEntity tmp = await context.Set<TEntity>().FindAsync(id);
                return tmp;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"GetByIdAsync {_typeName}");
                throw;
            }
        }

        public async Task<IEnumerable<TEntity>> GetByIdsAsync(IEnumerable<long> ids)
        {
            try
            {
                await using DbContext context = _contextFactory.CreateDbContext();
                List<TEntity> entities = await context.Set<TEntity>().Where(s => ids.Contains(s.Id)).ToListAsync();
                return entities;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"GetByIdsAsync {_typeName}");
                throw;
            }
        }

        public async Task<TEntity> SaveAsync(TEntity obj)
        {
            try
            {
                await using DbContext context = _contextFactory.CreateDbContext();

                await context.SingleMergeAsync(obj, operation =>
                {
                    operation.InsertKeepIdentity = true;
                    operation.IsCheckConstraintOnInsertDisabled = false;
                });

                return obj;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"SaveAsync {_typeName}");
                throw;
            }
        }

        public async Task<IEnumerable<TEntity>> SaveAsync(IReadOnlyList<TEntity> objs)
        {
            try
            {
                await using DbContext context = _contextFactory.CreateDbContext();

                await context.BulkMergeAsync(objs, operation =>
                {
                    operation.InsertKeepIdentity = true;
                    operation.IsCheckConstraintOnInsertDisabled = false;
                });
                return objs;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"SaveAsync {_typeName}");
                throw;
            }
        }

        public async Task DeleteByIdAsync(long id)
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
                _logger.LogError(e, $"DeleteByIdAsync {_typeName}");
                throw;
            }
        }

        public async Task DeleteByIdsAsync(IEnumerable<long> ids)
        {
            try
            {
                await using DbContext context = _contextFactory.CreateDbContext();
                foreach (long id in ids)
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
                _logger.LogError(e, $"DeleteByIdsAsync {_typeName}");
                throw;
            }
        }
    }
}