using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;

namespace WingsEmu.Health
{
    public class MaintenanceDeactivateMessageConsumer : IMessageConsumer<ServiceMaintenanceDeactivateMessage>
    {
        private readonly IMaintenanceManager _maintenanceManager;

        public MaintenanceDeactivateMessageConsumer(IMaintenanceManager maintenanceManager) => _maintenanceManager = maintenanceManager;

        public async Task HandleAsync(ServiceMaintenanceDeactivateMessage notification, CancellationToken token)
        {
            if (notification.IsGlobal)
            {
                _maintenanceManager.DeactivateMaintenance();
                return;
            }

            if (notification.TargetServiceName != _maintenanceManager.ServiceName)
            {
                return;
            }

            _maintenanceManager.DeactivateMaintenance();
        }
    }
}