using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using Plugin.MongoLogs.Services;
using Plugin.PlayerLogs;

namespace Plugin.MongoLogs.Utils
{
    public class GenericLogConsumer<T> : IMessageConsumer<T> where T : IPlayerActionLogMessage
    {
        private readonly MongoLogsBackgroundService _mongoLogsBackgroundService;

        public GenericLogConsumer(MongoLogsBackgroundService mongoLogsBackgroundService) => _mongoLogsBackgroundService = mongoLogsBackgroundService;

        public Task HandleAsync(T notification, CancellationToken token)
        {
            _mongoLogsBackgroundService.AddLogsToInsertionQueue(notification);
            return Task.CompletedTask;
        }
    }
}