using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.Events;
using PhoenixLib.ServiceBus.Extensions;
using Plugin.RainbowBattle.Command;
using Plugin.RainbowBattle.Managers;
using Plugin.RainbowBattle.RecurrentJob;
using WingsAPI.Communication.RainbowBattle;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Plugins;
using WingsEmu.Commands.Interfaces;
using WingsEmu.Game.RainbowBattle;

namespace Plugin.RainbowBattle
{
    public class RainbowBattlePluginCore : IGameServerPlugin
    {
        public string Name => nameof(RainbowBattlePluginCore);

        public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
        {
            services.AddEventHandlersInAssembly<RainbowBattlePlugin>();
            services.AddSingleton<IRainbowBattleManager, RainbowBattleManager>();
            services.AddMessageSubscriber<RainbowBattleStartMessage, RainbowBattleStartMessageConsumer>();
            services.AddMessageSubscriber<RainbowBattleLeaverBusterResetMessage, RainbowBattleLeaverBusterResetMessageConsumer>();
            services.AddSingleton<IRainbowFactory, RainbowFactory>();
            if (gameServer.Type == GameChannelType.ACT_4)
            {
                return;
            }

            services.AddHostedService<RainbowBattleSystem>();
        }
    }

    public class RainbowBattlePlugin : IGamePlugin
    {
        private readonly ICommandContainer _commandContainer;

        public RainbowBattlePlugin(ICommandContainer commandContainer) => _commandContainer = commandContainer;

        public string Name => nameof(RainbowBattlePlugin);

        public void OnLoad()
        {
            _commandContainer.AddModule<RainbowBattleCommandModule>();
        }
    }
}