using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.ServiceBus.Extensions;
using Plugin.PlayerLogs;

namespace DiscordNotifier.Formatting
{
    public static class DiscordLogExtensions
    {
        public static void AddDiscordFormattedLog<TMessage, TFormatter>(this IServiceCollection services)
        where TMessage : class, IPlayerActionLogMessage
        where TFormatter : class, IDiscordLogFormatter<TMessage>
        {
            services.AddMessageSubscriber<TMessage, GenericDiscordLogConsumer<TMessage>>();
            services.AddSingleton<IDiscordLogFormatter<TMessage>, TFormatter>();
        }

        public static void AddDiscordEmbedFormattedLog<TMessage, TFormatter>(this IServiceCollection services)
        where TMessage : class, IPlayerActionLogMessage
        where TFormatter : class, IDiscordEmbedLogFormatter<TMessage>
        {
            services.AddMessageSubscriber<TMessage, GenericDiscordEmbedLogConsumer<TMessage>>();
            services.AddSingleton<IDiscordEmbedLogFormatter<TMessage>, TFormatter>();
        }
    }
}