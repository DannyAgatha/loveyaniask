using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;

namespace WingsEmu.Health
{
    public class MaintenanceActivateMessageConsumer : IMessageConsumer<ServiceMaintenanceActivateMessage>
    {
        private readonly IMaintenanceManager _maintenanceManager;

        public MaintenanceActivateMessageConsumer(IMaintenanceManager maintenanceManager) => _maintenanceManager = maintenanceManager;

        public async Task HandleAsync(ServiceMaintenanceActivateMessage notification, CancellationToken token)
        {
            if (notification.IsGlobal)
            {
                _maintenanceManager.ActivateMaintenance();
                return;
            }

            if (notification.TargetServiceName != _maintenanceManager.ServiceName)
            {
                return;
            }

            _maintenanceManager.ActivateMaintenance();
        }
    }
}