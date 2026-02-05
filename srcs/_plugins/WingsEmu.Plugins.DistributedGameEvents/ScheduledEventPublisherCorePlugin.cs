using Microsoft.Extensions.DependencyInjection;
using WingsAPI.Plugins;
using WingsEmu.Plugins.DistributedGameEvents.BotMessages.Extensions;

namespace WingsEmu.Plugins.DistributedGameEvents
{
    public class ScheduledEventPublisherCorePlugin : IDependencyInjectorPlugin
    {
        public string Name => nameof(ScheduledEventPublisherCorePlugin);


        public void AddDependencies(IServiceCollection services)
        {
            services.AddRecurrentBotMessagesSchedulerModule();
        }
    }
}