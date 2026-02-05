using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.Events.Internal;
using PhoenixLib.Logging;

namespace PhoenixLib.Events
{
    public static class DependencyInjection
    {
        public static void AddEventHandlersInAssembly<T>(this IServiceCollection services)
        {
            Type[] types = typeof(T).Assembly.GetTypesImplementingInterface(typeof(IAsyncEventProcessor<>));
            foreach (Type handlerType in types)
            {
                services.AddTransient(handlerType);
                Type handlerInterface = handlerType.GetInterfaces().First(s => s.IsGenericType && s.GetGenericTypeDefinition() == typeof(IAsyncEventProcessor<>));
                Log.Debug($"[EVENT_HANDLER] Added IEventHandler<{handlerInterface.GetGenericArguments()[0].Name}> : {handlerType}");
                services.AddTransient(handlerInterface, handlerType);
            }
        }

        public static void AddEventPipeline(this IServiceCollection services)
        {
            services.AddSingleton<IAsyncEventPipeline, AsyncEventPipeline>();
            services.AddSingleton<IEventProcessorPipeline, EventPipeline>();
        }

        public static void AddEventProcessorsInAssembly<T>(this IServiceCollection services)
        {
            Type[] types = typeof(T).Assembly.GetTypesImplementingInterface(typeof(IEventProcessor<>));
            foreach (Type handlerType in types)
            {
                services.AddTransient(handlerType);
                Type handlerInterface = handlerType.GetInterfaces().First(s => s.IsGenericType && s.GetGenericTypeDefinition() == typeof(IEventProcessor<>));
                Log.Debug($"[EVENT_HANDLER] Added IEventProcessor<{handlerInterface.GetGenericArguments()[0].Name}> : {handlerType}");
                services.AddTransient(handlerInterface, handlerType);
            }
        }
    }
}