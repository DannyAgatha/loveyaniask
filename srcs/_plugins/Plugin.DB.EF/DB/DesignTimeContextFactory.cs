// NosEmu
// 


using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace Plugin.Database.DB
{
    public class DesignTimeContextFactory : IDesignTimeDbContextFactory<GameContext>
    {
        public GameContext CreateDbContext(string[] args)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            var optionsBuilder = new DbContextOptionsBuilder<GameContext>();

            optionsBuilder.UseNpgsql(new DatabaseConfiguration().ToString());
            return new GameContext(optionsBuilder.Options);
        }
    }
}