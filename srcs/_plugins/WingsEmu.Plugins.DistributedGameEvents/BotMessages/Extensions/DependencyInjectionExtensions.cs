// NosEmu
// 


using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.Configuration;
using PhoenixLib.Events;
using PhoenixLib.ServiceBus.Extensions;

namespace WingsEmu.Plugins.DistributedGameEvents.BotMessages.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static void AddRecurrentBotMessagesGameModule(this IServiceCollection services)
        {
            services.AddEventHandlersInAssembly<BotMessageConsumer>();
            services.AddMessageSubscriber<BotMessageMessage, BotMessageConsumer>();
        }

        public static void AddRecurrentBotMessagesSchedulerModule(this IServiceCollection services)
        {
            services.AddMultipleConfigurationOneFile<ScheduledBotMessageConfiguration>("bot_messages");
        }
    }
}