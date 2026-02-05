using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.Events;
using PhoenixLib.ServiceBus.Extensions;
using WingsEmu.Game._packetHandling;

namespace Plugin.PlayerLogs.Core
{
    public static class LogDependencyInjectionExtensions
    {
        public static void AddPlayerLog<TEvent, TMessage, TEnricher>(this IServiceCollection services)
        where TMessage : IPlayerActionLogMessage, new()
        where TEvent : PlayerEvent
        where TEnricher : class, ILogMessageEnricher<TEvent, TMessage>
        {
            services.AddSingleton<IAsyncEventProcessor<TEvent>, GenericPlayerGameEventToLogProcessor<TEvent, TMessage>>();
            services.AddSingleton<IPlayerEventLogMessageFactory<TEvent, TMessage>, GenericPlayerEventLogMessageFactory<TEvent, TMessage>>();
            services.AddSingleton<ILogMessageEnricher<TEvent, TMessage>, TEnricher>();
            services.AddMessagePublisher<TMessage>();
        }
    }
}