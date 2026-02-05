using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using Plugin.MongoLogs.Services;
using WingsAPI.Communication.Services.Messages;

namespace LogsServer.Consumers
{
    public class ServiceFlushAllMessageConsumer : IMessageConsumer<ServiceFlushAllMessage>
    {
        private readonly MongoLogsBackgroundService _mongoLogsBackgroundService;

        public ServiceFlushAllMessageConsumer(MongoLogsBackgroundService mongoLogsBackgroundService) => _mongoLogsBackgroundService = mongoLogsBackgroundService;

        public async Task HandleAsync(ServiceFlushAllMessage notification, CancellationToken token)
        {
            await _mongoLogsBackgroundService.ProcessMain(token);
        }
    }
}