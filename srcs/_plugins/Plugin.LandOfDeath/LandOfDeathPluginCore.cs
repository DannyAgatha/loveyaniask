using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using Plugin.LandOfDeath.RecurrentJob;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Plugins;
using WingsEmu.Commands.Interfaces;
using WingsEmu.Game.LandOfDeath;

namespace Plugin.LandOfDeath
{
    public class LandOfDeathPluginCore : IGameServerPlugin
    {
        public string Name => nameof(LandOfDeathPluginCore);

        public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
        {
            services.AddEventHandlersInAssembly<LandOfDeathPlugin>();
            services.AddSingleton<ILandOfDeathManager, LandOfDeathManager>();
            services.AddTransient<ILandOfDeathFactory, LandOfDeathFactory>();
            if (gameServer.Type == GameChannelType.ACT_4)
            {
                Log.Debug("Not loading in Act4 plugin because this is an Act4 channel.");
                return;
            }

            services.AddHostedService<LandOfDeathSystem>();
        }
    }

    public class LandOfDeathPlugin : IGamePlugin
    {
        private readonly ICommandContainer _commandContainer;

        public LandOfDeathPlugin(ICommandContainer commandContainer) => _commandContainer = commandContainer;

        public string Name => nameof(LandOfDeathPlugin);

        public void OnLoad()
        {
            _commandContainer.AddModule<LandOfDeathModule>();
        }
    }
}