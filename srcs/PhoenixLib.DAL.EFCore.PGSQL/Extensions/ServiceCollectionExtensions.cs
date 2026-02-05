using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace PhoenixLib.DAL.EFCore.PGSQL.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void TryAddStringRepository<TEntity, TDbContext>(this IServiceCollection services)
        where TEntity : class, IStringKeyEntity, new()
        where TDbContext : DbContext
        {
            services.TryAddTransient<IGenericStringRepository<TEntity>, GenericStringRepository<TEntity, TDbContext>>();
        }


        public static void TryAddLongRepository<TDto, TEntity, TDbContext>(this IServiceCollection services)
        where TEntity : class, ILongEntity, new()
        where TDto : class, ILongDto, new()
        where TDbContext : DbContext
        {
            services.TryAddLongRepository<TEntity, TDbContext>();
            services.TryAddTransient<IGenericAsyncLongRepository<TDto>, GenericMappedLongRepository<TEntity, TDto>>();
        }

        public static void TryAddLongRepository<TEntity, TDbContext>(this IServiceCollection services)
        where TEntity : class, ILongEntity, new()
        where TDbContext : DbContext
        {
            services.TryAddTransient<IGenericAsyncLongRepository<TEntity>, GenericLongRepository<TEntity, TDbContext>>();
        }

        public static void TryAddOldMappedLongRepository<TDto, TEntity, TDbContext>(this IServiceCollection services)
        where TEntity : class, ILongEntity, new()
        where TDto : class, ILongDto, new()
        where TDbContext : DbContext
        {
            services.TryAddTransient<IGenericAsyncLongRepository<TDto>, GenericOldMappedLongRepository<TEntity, TDto, TDbContext>>();
        }


        public static void TryAddUuidRepository<TDto, TEntity, TDbContext>(this IServiceCollection services)
        where TEntity : class, IUuidEntity, new()
        where TDto : class, IUuidDto, new()
        where TDbContext : DbContext
        {
            services.TryAddUuidRepository<TEntity, TDbContext>();
            services.TryAddTransient<IGenericAsyncUuidRepository<TDto>, GenericMappedUuidRepository<TEntity, TDto>>();
        }

        public static void TryAddUuidRepository<TEntity, TDbContext>(this IServiceCollection services)
        where TEntity : class, IUuidEntity, new()
        where TDbContext : DbContext
        {
            services.TryAddTransient<IGenericAsyncUuidRepository<TEntity>, GenericUuidRepository<TEntity, TDbContext>>();
        }

        public static void TryAddOldMappedUuidRepository<TDto, TEntity, TDbContext>(this IServiceCollection services)
        where TEntity : class, IUuidEntity, new()
        where TDto : class, IUuidDto, new()
        where TDbContext : DbContext
        {
            services.TryAddTransient<IGenericAsyncUuidRepository<TDto>, GenericOldMappedUuidRepository<TEntity, TDto, TDbContext>>();
        }


        public static void TryAddIntRepository<TDto, TEntity, TDbContext>(this IServiceCollection services)
        where TEntity : class, IIntEntity, new()
        where TDto : class, IIntDto, new()
        where TDbContext : DbContext
        {
            services.TryAddIntRepository<TEntity, TDbContext>();
            services.TryAddTransient<IGenericAsyncIntRepository<TDto>, GenericMappedIntRepository<TEntity, TDto>>();
        }

        public static void TryAddIntRepository<TEntity, TDbContext>(this IServiceCollection services)
        where TEntity : class, IIntEntity, new()
        where TDbContext : DbContext
        {
            services.TryAddTransient<IGenericAsyncIntRepository<TEntity>, GenericIntRepository<TEntity, TDbContext>>();
        }

        public static void TryAddOldMappedIntRepository<TDto, TEntity, TDbContext>(this IServiceCollection services)
        where TEntity : class, IIntEntity, new()
        where TDto : class, IIntDto, new()
        where TDbContext : DbContext
        {
            services.TryAddTransient<IGenericAsyncIntRepository<TDto>, GenericOldMappedIntRepository<TEntity, TDto, TDbContext>>();
        }
    }
}