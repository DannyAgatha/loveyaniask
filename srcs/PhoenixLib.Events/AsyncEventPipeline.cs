using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Logging;

namespace PhoenixLib.Events
{
    internal class AsyncEventPipeline : IAsyncEventPipeline
    {
        private static readonly MethodInfo ProcessAsyncGenericMethodInfo =
            typeof(AsyncEventPipeline).GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(s => s.Name == nameof(ProcessEventAsync) && s.IsGenericMethod);

        private readonly IServiceProvider _serviceProvider;

        public AsyncEventPipeline(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public async Task ProcessEventAsync(IAsyncEvent notification)
        {
            MethodInfo method = ProcessAsyncGenericMethodInfo.MakeGenericMethod(notification.GetType());
            await (Task)method.Invoke(this, new object[] { notification, CancellationToken.None });
        }

        public async Task ProcessEventAsync<T>(T notification, CancellationToken cancellationToken = default) where T : IAsyncEvent
        {
            try
            {
                IEnumerable<IAsyncEventProcessor<T>> handlers = _serviceProvider.GetServices<IAsyncEventProcessor<T>>();
                foreach (IAsyncEventProcessor<T> handler in handlers)
                {
                    await handler.HandleAsync(notification, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Log.Error("ProcessEventAsync", e);
            }
        }
    }
}