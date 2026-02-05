// NosEmu
// 


using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsEmu.Health;

namespace Master.Services.Maintenance
{
    public class StatusRefreshMessageConsumer : IMessageConsumer<ServiceStatusUpdateMessage>
    {
        private readonly IStatusManager _statusManager;

        public StatusRefreshMessageConsumer(IStatusManager statusManager) => _statusManager = statusManager;

        public async Task HandleAsync(ServiceStatusUpdateMessage notification, CancellationToken token) =>
            _statusManager.UpdateStatus(new ServiceStatus
            {
                Status = notification.StatusType,
                LastUpdate = notification.LastUpdate,
                ServiceName = notification.ServiceName
            });
    }
}