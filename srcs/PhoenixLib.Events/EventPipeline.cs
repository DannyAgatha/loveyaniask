using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PhoenixLib.Logging;

namespace PhoenixLib.Events
{
    internal class EventPipeline : IEventProcessorPipeline
    {
        private static readonly MethodInfo ProcessAsyncGenericMethodInfo =
            typeof(EventPipeline).GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(s => s.Name == nameof(ProcessEvent) && s.IsGenericMethod);

        private readonly IServiceProvider _serviceProvider;

        public EventPipeline(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public void ProcessEvent(IEvent notification)
        {
            MethodInfo method = ProcessAsyncGenericMethodInfo.MakeGenericMethod(notification.GetType());
            method.Invoke(this, new object?[] { notification });
        }

        public void ProcessEvent<T>(T eventNotification) where T : IEvent
        {
            IEnumerable<IEventProcessor<T>> handlers = _serviceProvider.GetServices<IEventProcessor<T>>();

            try
            {
                foreach (IEventProcessor<T> handler in handlers)
                {
                    handler.Handle(eventNotification);
                }
            }
            catch (Exception e)
            {
                Log.Error("ProcessEventAsync", e);
            }
        }
    }
}