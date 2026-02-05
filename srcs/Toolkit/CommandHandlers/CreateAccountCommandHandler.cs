using System;
using System.Linq;
using System.Threading.Tasks;
using dotenv.net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhoenixLib.Extensions;
using PhoenixLib.Logging;
using Plugin.Database;
using Plugin.Database.DB;
using Plugin.Database.Entities.Account;
using Toolkit.Commands;
using WingsAPI.Plugins;
using WingsAPI.Plugins.Exceptions;
using WingsEmu.DTOs.Account;

namespace Toolkit.CommandHandlers;

public class CreateAccountCommandHandler
{
    private static ServiceProvider BuildCoreContainer(CreateAccountCommand command)
    {
        DotEnv.Load(new DotEnvOptions(true, new[] { command.EnvFile }));
        var pluginBuilder = new ServiceCollection();
        pluginBuilder.AddTransient<IDependencyInjectorPlugin, DatabasePlugin>();
        ServiceProvider container = pluginBuilder.BuildServiceProvider();

        var coreBuilder = new ServiceCollection();
        foreach (IDependencyInjectorPlugin plugin in container.GetServices<IDependencyInjectorPlugin>())
        {
            try
            {
                Log.Debug($"[PLUGIN_LOADER] Loading generic plugin {plugin.Name}...");
                plugin.AddDependencies(coreBuilder);
            }
            catch (PluginException e)
            {
                Log.Error("[PLUGIN_LOADER] Add dependencies", e);
            }
        }

        coreBuilder.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();
            builder.AddFilter("Microsoft", LogLevel.Warning);
            builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
        });

        return coreBuilder.BuildServiceProvider();
    }

    public static async Task<int> HandleAsync(CreateAccountCommand command)
    {
        await using ServiceProvider coreContainer = BuildCoreContainer(command);
        try
        {
            IDbContextFactory<GameContext> factory = coreContainer.GetRequiredService<IDbContextFactory<GameContext>>();
            await using GameContext context = factory.CreateDbContext();
            await context.Database.MigrateAsync();
            if (context.Account.Any())
            {
                Log.Info("[DEFAULT ACCOUNT] Accounts were already present!");
                return 0;
            }

            context.Account.Add(new AccountEntity
            {
                Authority = AuthorityType.DEV,
                Language = AccountLanguage.EN,
                Name = "Admin",
                Password = "test".ToSha512()
            });

            context.Account.Add(new AccountEntity
            {
                Authority = AuthorityType.DEV,
                Language = AccountLanguage.EN,
                Name = "Test",
                Password = "test".ToSha512()
            });
            await context.SaveChangesAsync();
            Log.Info("[DEFAULT ACCOUNT] Accounts created!");
            return 0;
        }
        catch (Exception e)
        {
            Log.Error("[DEFAULT ACCOUNT] Error", e);
            return 1;
        }
    }
}