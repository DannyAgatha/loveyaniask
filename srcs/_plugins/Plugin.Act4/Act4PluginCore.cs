using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PhoenixLib.Configuration;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus.Extensions;
using Plugin.Act4.Manager;
using Plugin.Act4.RecurrentJob;
using Plugin.Act4.Scripting.Validator;
using WingsAPI.Communication.Caligor;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Plugins;
using WingsAPI.Scripting;
using WingsAPI.Scripting.LUA;
using WingsAPI.Scripting.ScriptManager;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Act4.Configuration;
using WingsEmu.Game.Managers;

namespace Plugin.Act4;

public class Act4PluginCore : IGameServerPlugin
{
    public string Name => nameof(Act4PluginCore);

    public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
    {
        // TODO: Plz, when we have warmup we should move those "AddSingleton" down, so it only gets loaded when necessary
        // Dungeon Script Cache
        services.AddSingleton<SDungeonValidator>();
        services.AddSingleton<IDungeonScriptManager, DungeonScriptManager>();
        services.AddSingleton<IDungeonManager, DungeonManager>();
        services.AddFileConfiguration<Act4DungeonsConfiguration>();
        services.AddSingleton<IAct4FlagManager, Act4FlagManager>();
        services.AddFileConfiguration<Act4CaligorConfiguration>();
        services.AddFileConfiguration<Act4Configuration>();
        services.AddSingleton<IAct4CaligorManager, Act4CaligorManager>();
        
        
        // TODO: Remove building twice the ServiceProvider when we have another method to obtain server's type
        if (gameServer.Type != GameChannelType.ACT_4)
        {
            Log.Debug("Not loading Act4 plugin because this is not an Act4 channel.");
            return;
        }

        services.AddEventHandlersInAssembly<Act4PluginCore>();

        services.AddFileConfiguration<Act4Configuration>();

        services.AddSingleton<IAct4Manager, Act4Manager>();
        services.AddHostedService<Act4System>();

        services.AddSingleton<IAct4DungeonManager, Act4DungeonManager>();
        services.AddHostedService<Act4DungeonSystem>();
        
        services.AddHostedService<Act4Caligor>();
        services.AddMessageSubscriber<CaligorStartMessage, CaligorStartMessageConsumer>();
        services.AddMessageSubscriber<Act4PercentageMessage, Act4PercentageMessageConsumer>();

        services.TryAddSingleton(x =>
        {
            IConfigurationPathProvider config = x.GetRequiredService<IConfigurationPathProvider>();
            return new ScriptFactoryConfiguration
            {
                RootDirectory = config.GetConfigurationPath("scripts"),
                LibDirectory = config.GetConfigurationPath("scripts/lib")
            };
        });


        // script factory
        services.TryAddSingleton<IScriptFactory, LuaScriptFactory>();

        // factory
        services.AddSingleton<IDungeonFactory, DungeonFactory>();
    }
}