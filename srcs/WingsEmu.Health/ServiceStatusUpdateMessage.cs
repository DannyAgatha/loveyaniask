using System;
using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsEmu.Health
{
    /// <summary>
    ///     This message just acts as data holder not as a command like <see cref="ServiceMaintenanceActivateMessage" /> or
    ///     <see cref="ServiceMaintenanceDeactivateMessage" />
    /// </summary>
    [MessageType("service.status.update")]
    public class ServiceStatusUpdateMessage : IMessage
    {
        public string ServiceName { get; init; }
        public ServiceStatusType StatusType { get; init; }
        public DateTime LastUpdate { get; init; }
    }
}