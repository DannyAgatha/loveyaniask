// NosEmu
// 


using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PhoenixLib.Logging;
using Plugin.Database.DB;

namespace Plugin.Database.Extensions
{
    public static class GameContextFactoryExtensions
    {
        public static async Task<bool> TryMigrateAsync(this IDbContextFactory<GameContext> contextFactory)
        {
            await using GameContext context = contextFactory.CreateDbContext();
            try
            {
                await context.Database.MigrateAsync();
                Log.Info("DATABASE_INITIALIZED");
            }
            catch (Exception ex)
            {
                Log.Error("DATABASE_NOT_UPTODATE", ex);
                return false;
            }

            return true;
        }
    }
}