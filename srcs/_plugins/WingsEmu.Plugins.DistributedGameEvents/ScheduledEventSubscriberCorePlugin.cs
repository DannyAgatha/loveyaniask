using Microsoft.Extensions.DependencyInjection;
using WingsAPI.Plugins;
using WingsEmu.Plugins.DistributedGameEvents.BotMessages.Extensions;

namespace WingsEmu.Plugins.DistributedGameEvents
{
    public class ScheduledEventSubscriberCorePlugin : IGameServerPlugin
    {
        public string Name => nameof(ScheduledEventSubscriberCorePlugin);


        public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
        {
            services.AddRecurrentBotMessagesGameModule();
        }
    }
}