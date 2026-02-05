using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PhoenixLib.ServiceBus;

namespace WingsEmu.Health
{
    public class HealthCheckHostedService : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(Convert.ToInt32(Environment.GetEnvironmentVariable("WINGSEMU_HEALTHCHECK_INTERVAL_SECONDS") ?? "3"));
        private readonly IMaintenanceManager _maintenanceManager;

        private readonly IMessagePublisher<ServiceStatusUpdateMessage> _publisher;

        public HealthCheckHostedService(IMessagePublisher<ServiceStatusUpdateMessage> publisher, IMaintenanceManager maintenanceManager)
        {
            _publisher = publisher;
            _maintenanceManager = maintenanceManager;
        }

        public string ServiceName => _maintenanceManager.ServiceName;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _publisher.PublishAsync(new ServiceStatusUpdateMessage
                {
                    ServiceName = ServiceName,
                    StatusType = _maintenanceManager.IsMaintenanceActive ? ServiceStatusType.UNDER_MAINTENANCE : ServiceStatusType.ONLINE,
                    LastUpdate = DateTime.UtcNow
                });
                await Task.Delay(Interval, stoppingToken);
            }
        }
    }
}