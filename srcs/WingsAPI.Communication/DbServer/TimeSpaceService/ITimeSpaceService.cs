using System.ServiceModel;
using System.Threading.Tasks;

namespace WingsAPI.Communication.DbServer.TimeSpaceService
{
    [ServiceContract]
    public interface ITimeSpaceService
    {
        [OperationContract]
        ValueTask<TimeSpaceIsNewRecordResponse> IsNewRecord(TimeSpaceIsNewRecordRequest request);

        [OperationContract]
        ValueTask<EmptyResponse> SetNewRecord(TimeSpaceNewRecordRequest request);

        [OperationContract]
        ValueTask<TimeSpaceRecordResponse> GetTimeSpaceRecord(TimeSpaceRecordRequest request);
    }
}