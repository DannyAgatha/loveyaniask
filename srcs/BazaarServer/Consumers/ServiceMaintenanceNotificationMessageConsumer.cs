using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Services.Messages;
using WingsEmu.Health;

namespace BazaarServer.Consumers
{
    public class ServiceMaintenanceNotificationMessageConsumer : IMessageConsumer<ServiceMaintenanceNotificationMessage>
    {
        private readonly IMaintenanceManager _maintenanceManager;

        public ServiceMaintenanceNotificationMessageConsumer(IMaintenanceManager maintenanceManager) => _maintenanceManager = maintenanceManager;

        public async Task HandleAsync(ServiceMaintenanceNotificationMessage notification, CancellationToken token)
        {
            if (notification.TimeLeft <= TimeSpan.FromMinutes(5))
            {
                _maintenanceManager.ActivateMaintenance();
            }
        }
    }
}