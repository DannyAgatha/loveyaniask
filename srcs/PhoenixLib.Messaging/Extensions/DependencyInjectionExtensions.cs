// NosEmu
// 


using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PhoenixLib.ServiceBus.Internal.Routing;
using PhoenixLib.ServiceBus.MQTT;
using PhoenixLib.ServiceBus.Protocol;
using PhoenixLib.ServiceBus.Routing;

namespace PhoenixLib.ServiceBus.Extensions
{
    public static class DependencyInjectionExtensions
    {
        internal static string ToLowerPure(this string str)
        {
            return string.Concat(str.Select(s => !char.IsLetterOrDigit(s) ? '/' : char.IsUpper(s) ? char.ToLower(s) : s));
        }


        public static void AddMqttConfigurationFromEnv(this IServiceCollection services)
        {
            services.TryAddSingleton(s => new MqttConfiguration(
                Environment.GetEnvironmentVariable("MQTT_BROKER_ADDRESS") ?? "localhost",
                Environment.GetEnvironmentVariable("MQTT_BROKER_CLIENT_NAME") ?? "client-" + Guid.NewGuid(),
                Convert.ToInt32(Environment.GetEnvironmentVariable("MQTT_BROKER_PORT") ?? "1883")
            ));
        }

        internal static void AddMessageService(this IServiceCollection services)
        {
            services.TryAddSingleton<IRoutingInformationFactory, RoutingInformationFactory>();
            services.TryAddSingleton<IMessageRouter, MessageRouter>();
            services.TryAddSingleton<IMessageSerializer, CloudEventsJsonMessageSerializer>();
            services.TryAddSingleton<IServiceBusInstance, ServiceBusInstance>();
            services.TryAddSingleton<IMessagingService, MqttMessagingService>();
        }

        internal static MessageTypeAttribute GetMessageAttributes<T>() => typeof(T).GetMessageAttributes();

        internal static void RegisterMessage<T>(this IServiceCollection services) where T : IMessage
        {
            GetMessageAttributes<T>();
            services.AddSingleton<ISubscribedMessage, GenericSubscribedMessage<T>>();
        }

        public static void AddMessagePublisher<T>(this IServiceCollection services) where T : IMessage
        {
            services.AddMessageService();

            // adds the event publisher
            services.TryAddSingleton<IMessagePublisher<T>, GenericMessagePublisher<T>>();
        }

        public static void AddMessageSubscriber<TMessage, TMessageConsumer>(this IServiceCollection services)
        where TMessage : class, IMessage
        where TMessageConsumer : class, IMessageConsumer<TMessage>
        {
            services.AddMessageService();
            services.RegisterMessage<TMessage>();

            // Message -> event pipeline forwarder
            services.AddSingleton<IMessageConsumer<TMessage>, TMessageConsumer>();
        }
    }
}