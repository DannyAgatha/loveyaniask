using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using PhoenixLib.Logging;
using Plugin.MongoLogs.Utils;
using WingsEmu.Game.Logs;

namespace Plugin.MongoLogs.Services
{
    public sealed class MongoLogsBackgroundService : BackgroundService
    {
        private static readonly MethodInfo Method = typeof(MongoLogsBackgroundService).GetMethod(nameof(InsertLog), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);
        private readonly IMongoDatabase _database;
        private readonly ConcurrentDictionary<Type, MethodInfo> _methodsCache = new();
        private readonly ConcurrentQueue<(Type, IPlayerActionLog)> _queue = new();

        public MongoLogsBackgroundService(MongoLogsConfiguration mongoLogsConfiguration)
        {
            var client = new MongoClient(mongoLogsConfiguration.ToString());
            _database = client.GetDatabase(mongoLogsConfiguration.DbName);
        }

        public void AddLogsToInsertionQueue<T>(T log) where T : IPlayerActionLog
        {
            _queue.Enqueue((typeof(T), log));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessMain(stoppingToken);
            }
        }

        public async Task ProcessMain(CancellationToken stoppingToken)
        {
            if (_queue.IsEmpty)
            {
                Log.Debug("Queue is empty. Nothing to send to mongo.");
                await Task.Delay(Interval, stoppingToken);
            }

            while (_queue.TryDequeue(out (Type, IPlayerActionLog) log))
            {
                try
                {
                    Type type = log.Item1;
                    MethodInfo method = _methodsCache.GetOrAdd(type, Method.MakeGenericMethod(type));
                    await (Task)method.Invoke(this, new object?[] { _database, log.Item2 });
                }
                catch (Exception e)
                {
                    Log.Error("Couldn't send that action log message to mongodb. See the following exception:", e);
                }
            }
        }

        private static async Task InsertLog<T>(IMongoDatabase database, T log) where T : IPlayerActionLog
        {
            await database.InsertLogAsync(log);
        }
    }
}