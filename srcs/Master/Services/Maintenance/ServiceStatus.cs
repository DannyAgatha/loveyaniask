using System;
using WingsEmu.Health;

namespace Master.Services.Maintenance
{
    public class ServiceStatus
    {
        public string ServiceName { get; set; }
        public ServiceStatusType Status { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}