using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsEmu.Health
{
    [MessageType("service.status.maintenance.deactivate")]
    public class ServiceMaintenanceDeactivateMessage : IMessage
    {
        public string TargetServiceName { get; init; }
        public bool IsGlobal { get; init; }
    }
}