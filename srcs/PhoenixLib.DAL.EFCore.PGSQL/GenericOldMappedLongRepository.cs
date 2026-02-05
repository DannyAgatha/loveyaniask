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
    [Obsolete("Use GenericMappedLongRepository now")]
    public sealed class GenericOldMappedLongRepository<TEntity, TDto, TDbContext> : IGenericAsyncLongRepository<TDto>
    where TDto : class, ILongDto, new()
    where TEntity : class, ILongEntity, new()
    where TDbContext : DbContext
    {
        private readonly IDbContextFactory<TDbContext> _contextFactory;
        private readonly ILogger<GenericOldMappedLongRepository<TEntity, TDto, TDbContext>> _logger;
        private readonly IMapper<TEntity, TDto> _mapper;

        public GenericOldMappedLongRepository(IDbContextFactory<TDbContext> contextFactory,
            IMapper<TEntity, TDto> mapper, ILogger<GenericOldMappedLongRepository<TEntity, TDto, TDbContext>> logger)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<TDto>> GetAllAsync()
        {
            try
            {
                await using TDbContext context = _contextFactory.CreateDbContext();
                List<TEntity> tmp = await context.Set<TEntity>().ToListAsync();
                return _mapper.Map(tmp);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetAllAsync");
                throw;
            }
        }


        public async Task<TDto> GetByIdAsync(long id)
        {
            try
            {
                await using DbContext context = _contextFactory.CreateDbContext();
                TEntity tmp = await context.Set<TEntity>().FindAsync(id);
                return _mapper.Map(tmp);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetByIdAsync");
                throw;
            }
        }

        public async Task<IEnumerable<TDto>> GetByIdsAsync(IEnumerable<long> ids)
        {
            try
            {
                await using DbContext context = _contextFactory.CreateDbContext();
                List<TEntity> entities = await context.Set<TEntity>().Where(s => ids.Contains(s.Id)).ToListAsync();
                return _mapper.Map(entities);
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

                await context.SingleMergeAsync(entity, operation =>
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
                List<TEntity> entities = _mapper.Map(objs.ToList());
                await using (DbContext context = _contextFactory.CreateDbContext())
                {
                    await context.BulkMergeAsync(entities, operation =>
                    {
                        operation.InsertKeepIdentity = true;
                        operation.IsCheckConstraintOnInsertDisabled = false;
                    });
                }

                return _mapper.Map(entities);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "SaveAsyncBulk");
                throw;
            }
        }

        public async Task DeleteByIdAsync(long id)
        {
            try
            {
                await using DbContext context = _contextFactory.CreateDbContext();
                var model = new TEntity { Id = id };
                context.Set<TEntity>().Attach(model);
                context.Set<TEntity>().Remove(model);
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "DeleteByIdsAsync");
                throw;
            }
        }

        public async Task DeleteByIdsAsync(IEnumerable<long> ids)
        {
            try
            {
                await using DbContext context = _contextFactory.CreateDbContext();
                var toDelete = ids.Select(s => new TEntity { Id = s }).ToList();
                context.Set<TEntity>().RemoveRange(toDelete);
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