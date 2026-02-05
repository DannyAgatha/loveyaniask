using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus;
using WingsEmu.Game.Logs;

namespace Plugin.PlayerLogs
{
    public sealed class PlayerLogManager : BackgroundService, IPlayerLogManager
    {
        private static readonly MethodInfo PublishLogAsyncMethod = typeof(PlayerLogManager).GetMethod(nameof(PublishLogAsync), BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly ConcurrentDictionary<Type, MethodInfo> _publishLogsCache = new();
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);
        private readonly ConcurrentQueue<(IPlayerActionLog, Type)> _queue;

        private readonly IServiceProvider _serviceProvider;

        public PlayerLogManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _queue = new ConcurrentQueue<(IPlayerActionLog, Type)>();
        }

        public void AddLog<T>(T message) where T : IPlayerActionLog
        {
            _queue.Enqueue((message, typeof(T)));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_queue.IsEmpty)
                {
                    await Task.Delay(Interval, stoppingToken);
                }

                await PublishLogs();
                await Task.Delay(Interval, stoppingToken);
            }

            await PublishLogs();
        }

        private async Task PublishLogs()
        {
            while (_queue.TryDequeue(out (IPlayerActionLog, Type) result))
            {
                try
                {
                    IPlayerActionLog log = result.Item1;
                    Type logType = result.Item2;
                    MethodInfo logMethod = _publishLogsCache.GetOrAdd(logType, PublishLogAsyncMethod.MakeGenericMethod(logType));
                    await (Task)logMethod.Invoke(this, new object[] { log });
                }
                catch (Exception e)
                {
                    Log.Error("Couldn't publish that action log message. See the following exception:", e);
                }
            }
        }

        private async Task PublishLogAsync<T>(T log) where T : IPlayerActionLog, IMessage
        {
            IMessagePublisher<T> publisher = _serviceProvider.GetRequiredService<IMessagePublisher<T>>();
            await publisher.PublishAsync(log);
        }
    }
}