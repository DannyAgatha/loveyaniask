using System;

namespace WingsEmu.Health
{
    public class MaintenanceManager : IMaintenanceManager
    {
        private static readonly string _serviceName = Environment.GetEnvironmentVariable("WINGSEMU_HEALTHCHECK_SERVICENAME") ?? "wingsemu-service-" + Guid.NewGuid();
        public string ServiceName => _serviceName;
        public bool IsMaintenanceActive { get; private set; }

        public void ActivateMaintenance()
        {
            IsMaintenanceActive = true;
        }

        public void DeactivateMaintenance()
        {
            IsMaintenanceActive = false;
        }
    }
}