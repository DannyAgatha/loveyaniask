using Microsoft.Extensions.DependencyInjection;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsEmu.Game.Managers;
using WingsAPI.Plugins;
using PhoenixLib.Logging;
using Plugin.Act6.RecurrentJob;
using PhoenixLib.Events;
using WingsEmu.Game.Act6.Configuration;
using PhoenixLib.Configuration;
using WingsEmu.Game.Act6;

namespace Plugin.Act6
{
    public class Act6PluginCore : IGameServerPlugin
    {
        public string Name => nameof(Act6PluginCore);

        public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
        {
            if (gameServer.Type == GameChannelType.ACT_4)
            {
                Log.Debug("Not loading in Act4 plugin because this is an Act4 channel.");
                return;
            }

            services.AddEventHandlersInAssembly<Act6PluginCore>();

            services.AddFileConfiguration<Act6Configuration>();

            services.AddSingleton<IAct6Manager, Act6Manager>();
            services.AddSingleton<IAct6InstanceManager, Act6InstanceManager>();
            services.AddHostedService<Act6System>();
        }
    }
}
