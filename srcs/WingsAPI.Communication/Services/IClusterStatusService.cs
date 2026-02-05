using System.ServiceModel;
using System.Threading.Tasks;
using WingsAPI.Communication.Services.Requests;
using WingsAPI.Communication.Services.Responses;

namespace WingsAPI.Communication.Services
{
    [ServiceContract]
    public interface IClusterStatusService
    {
        [OperationContract]
        Task<ServiceGetAllResponse> GetAllServicesStatus(EmptyRpcRequest req);

        [OperationContract]
        Task<ServiceGetStatusByNameResponse> GetServiceStatusByNameAsync(ServiceBasicRequest req);

        [OperationContract]
        Task<BasicRpcResponse> EnableMaintenanceMode(ServiceBasicRequest req);

        [OperationContract]
        Task<BasicRpcResponse> DisableMaintenanceMode(ServiceBasicRequest req);

        [OperationContract]
        Task<BasicRpcResponse> ScheduleGeneralMaintenance(ServiceScheduleGeneralMaintenanceRequest maintenanceRequest);

        [OperationContract]
        Task<BasicRpcResponse> UnscheduleGeneralMaintenance(EmptyRpcRequest emptyRpcRequest);

        [OperationContract]
        Task<BasicRpcResponse> ExecuteGeneralEmergencyMaintenance(ServiceExecuteGeneralEmergencyMaintenanceRequest shutdownRequest);

        [OperationContract]
        Task<BasicRpcResponse> LiftGeneralMaintenance(EmptyRpcRequest shutdownRequest);
    }
}