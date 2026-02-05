using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PhoenixLib.DAL.EFCore.PGSQL
{
    /// <summary>
    ///     GenericAsyncMappedRepository is an asynchronous repository for a given Entity
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TDbContext"></typeparam>
    public class GenericStringRepository<TEntity, TDbContext> : IGenericStringRepository<TEntity>
    where TEntity : class, IStringKeyEntity, new()
    where TDbContext : DbContext
    {
        private readonly IDbContextFactory<TDbContext> _contextFactory;
        private readonly ILogger<GenericStringRepository<TEntity, TDbContext>> _logger;

        public GenericStringRepository(IDbContextFactory<TDbContext> contextFactory, ILogger<GenericStringRepository<TEntity, TDbContext>> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            try
            {
                await using DbContext context = _contextFactory.CreateDbContext();
                List<TEntity> tmp = await context.Set<TEntity>().ToListAsync();
                return tmp;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetAllAsync");
                throw;
            }
        }


        public async Task<TEntity> GetByIdAsync(string id)
        {
            try
            {
                await using DbContext context = _contextFactory.CreateDbContext();
                TEntity tmp = await context.Set<TEntity>().FindAsync(id);
                return tmp;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetByIdAsync");
                throw;
            }
        }

        public async Task<IEnumerable<TEntity>> GetByIdsAsync(IEnumerable<string> ids)
        {
            try
            {
                await using DbContext context = _contextFactory.CreateDbContext();
                List<TEntity> tmp = await context.Set<TEntity>().Where(s => ids.Contains(s.Id)).ToListAsync();
                return tmp;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetByIdsAsync");
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
                _logger.LogError(e, "SaveAsync");
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
                _logger.LogError(e, $"SaveAsync<{typeof(TEntity).Name}>");
                throw;
            }
        }

        public async Task DeleteByIdAsync(string id)
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

        public async Task DeleteByIdsAsync(IEnumerable<string> ids)
        {
            try
            {
                await using DbContext context = _contextFactory.CreateDbContext();
                foreach (string id in ids)
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