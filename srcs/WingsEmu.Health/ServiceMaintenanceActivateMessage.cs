using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsEmu.Health
{
    /// <summary>
    ///     This message acts as a request
    /// </summary>
    [MessageType("service.status.maintenance.activate")]
    public class ServiceMaintenanceActivateMessage : IMessage
    {
        public string TargetServiceName { get; init; }
        public bool IsGlobal { get; init; }
    }
}