using System.Collections.Generic;

namespace Master.Services.Maintenance
{
    public interface IStatusManager
    {
        ServiceStatus GetServiceByName(string serviceName);
        IReadOnlyList<ServiceStatus> GetAllServicesStatus();
        void UpdateStatus(ServiceStatus service);
    }
}