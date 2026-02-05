using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.Events;
using PhoenixLib.ServiceBus.Extensions;
using Plugin.Alzanor.Commands;
using Plugin.Alzanor.Managers;
using Plugin.Alzanor.RecurrentJob;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Plugins;
using WingsEmu.Commands.Interfaces;
using WingsEmu.Game.Alzanor;
using WingsEmu.Game.Alzanor.Communication;

namespace Plugin.Alzanor;

public class AlzanorPluginCore : IGameServerPlugin
{
    public string Name => nameof(AlzanorPluginCore);
    
    public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
    {
        services.AddEventHandlersInAssembly<AlzanorPlugin>();
        services.AddSingleton<IAlzanorManager, AlzanorManager>();
        services.AddMessageSubscriber<AlzanorStartMessage, AlzanorStartMessageConsumer>();
        services.AddSingleton<IAlzanorFactory, AlzanorFactory>();
        if (gameServer.ChannelId != 1)
        {
            return;
        }

        services.AddHostedService<AlzanorSystem>();
    }
}

public class AlzanorPlugin : IGamePlugin
{
    private readonly ICommandContainer _commandContainer;

    public AlzanorPlugin(ICommandContainer commandContainer) => _commandContainer = commandContainer;

    public string Name => nameof(AlzanorPlugin);

    public void OnLoad()
    {
        _commandContainer.AddModule<AlzanorCommandsModule>();
    }
}