using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PhoenixLib.DAL;
using PhoenixLib.DAL.EFCore.PGSQL;
using PhoenixLib.DAL.EFCore.PGSQL.Extensions;
using Plugin.Database.Auth.ClientVersion;
using Plugin.Database.Auth.HWID;
using Plugin.Database.DB;
using WingsAPI.Communication.Auth;
using Z.EntityFramework.Extensions;

namespace Plugin.Database.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static void UseZEfCoreExtensions(this IApplicationBuilder app)
        {
            EntityFrameworkManager.ContextFactory = context => app.ApplicationServices.GetRequiredService<IDbContextFactory<GameContext>>().CreateDbContext();
        }

        internal static void AddAsyncLongRepository<TDto, TEntity>(this IServiceCollection services)
        where TDto : class, ILongDto, new()
        where TEntity : class, ILongEntity, new()
        {
            services.TryAddLongRepository<TDto, TEntity, GameContext>();
        }

        internal static void AddAsyncIntRepository<TDto, TEntity>(this IServiceCollection services)
        where TDto : class, IIntDto, new()
        where TEntity : class, IIntEntity, new()
        {
            services.TryAddIntRepository<TDto, TEntity, GameContext>();
        }

        internal static void AddAsyncUuidRepository<TDto, TEntity>(this IServiceCollection services)
        where TDto : class, IUuidDto, new()
        where TEntity : class, IUuidEntity, new()
        {
            services.TryAddUuidRepository<TDto, TEntity, GameContext>();
        }

        public static void AddGameAuthDataAccess(this IServiceCollection services)
        {
            // client version
            services.AddAsyncLongRepository<AuthorizedClientVersionDto, AuthorizedClientVersionEntity>();
            services.TryAddTransient<IAuthorizedClientVersionRepository, EfAuthorizedClientVersionRepository>();
            // hwid
            services.TryAddTransient<IBlacklistedHwidDao, BlacklistedHwidDao>();
        }
    }
}