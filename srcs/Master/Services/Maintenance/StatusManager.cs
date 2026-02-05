using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Services.Messages;
using WingsEmu.Core.Extensions;

namespace Master.Services.Maintenance
{
    public class StatusManager : BackgroundService, IStatusManager
    {
        private static readonly TimeSpan Refresh = TimeSpan.FromSeconds(Convert.ToInt32(Environment.GetEnvironmentVariable("SERVICE_STATUS_REFRESH_IN_SECONDS") ?? "3"));
        private static readonly TimeSpan Expiration = TimeSpan.FromSeconds(Convert.ToInt32(Environment.GetEnvironmentVariable("SERVICE_STATUS_EXPIRATION_IN_SECONDS") ?? "5"));

        private readonly Dictionary<string, DateTime> _alerts = new();
        private readonly IMessagePublisher<ServiceDownMessage> _serviceDownPublisher;
        private readonly Dictionary<string, ServiceStatus> _serviceStatus = new();

        public StatusManager(IMessagePublisher<ServiceDownMessage> serviceDownPublisher) => _serviceDownPublisher = serviceDownPublisher;

        public void UpdateStatus(ServiceStatus service)
        {
            _serviceStatus[service.ServiceName] = service;
        }

        public ServiceStatus GetServiceByName(string serviceName) => _serviceStatus.GetOrDefault(serviceName);

        public IReadOnlyList<ServiceStatus> GetAllServicesStatus() => _serviceStatus.Values.ToList();


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Warn("[SERVICE_STATUS_MANAGER] Starting...");
            while (!stoppingToken.IsCancellationRequested)
            {
                var services = _serviceStatus.Values.ToList();
                Log.Debug($"[SERVICE_STATUS_MANAGER] Checking {services.Count} status...");

                DateTime dateTime = DateTime.UtcNow;
                foreach (ServiceStatus service in services)
                {
                    if (service.LastUpdate + Expiration > dateTime)
                    {
                        Log.Debug($"[SERVICE_STATUS_MANAGER] {service.ServiceName} OK...");
                        continue;
                    }

                    // add alert
                    Log.Warn($"[SERVICE_STATUS_MANAGER] {service.ServiceName} is offline");

                    if (!_alerts.TryGetValue(service.ServiceName, out DateTime lastAlert))
                    {
                        lastAlert = DateTime.MinValue;
                    }

                    if (lastAlert + TimeSpan.FromMinutes(5) > dateTime)
                    {
                        continue;
                    }

                    _alerts[service.ServiceName] = dateTime;
                    await _serviceDownPublisher.PublishAsync(new ServiceDownMessage
                    {
                        ServiceName = service.ServiceName,
                        LastUpdate = service.LastUpdate
                    });
                }

                await Task.Delay(Refresh, stoppingToken);
            }
        }
    }
}