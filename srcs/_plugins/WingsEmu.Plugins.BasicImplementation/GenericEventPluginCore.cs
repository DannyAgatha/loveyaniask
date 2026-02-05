using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PhoenixLib.Events;
using WingsAPI.Plugins;
using WingsEmu.Game.PrivateMapInstances;

namespace NosEmu.Plugins.BasicImplementations;

public class GenericEventPluginCore : IGameServerPlugin
{
    public string Name => nameof(GenericEventPluginCore);

    public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
    {
        services.AddEventHandlersInAssembly<GenericEventPluginCore>();
        services.TryAddSingleton<IPrivateMapInstanceManager, PrivateMapInstanceManager>();
        services.AddHostedService<PrivateMapInstanceSystem>();
    }
}