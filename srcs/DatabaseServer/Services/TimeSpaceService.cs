using System.Threading.Tasks;
using DatabaseServer.Managers;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.TimeSpaceService;

namespace DatabaseServer.Services
{
    public class TimeSpaceService : ITimeSpaceService
    {
        private readonly ITimeSpaceManager _timeSpaceManager;

        public TimeSpaceService(ITimeSpaceManager timeSpaceManager) => _timeSpaceManager = timeSpaceManager;

        public async ValueTask<TimeSpaceIsNewRecordResponse> IsNewRecord(TimeSpaceIsNewRecordRequest request) => new()
        {
            IsNewRecord = await _timeSpaceManager.IsNewRecord(request.TimeSpaceId, request.Record)
        };

        public async ValueTask<EmptyResponse> SetNewRecord(TimeSpaceNewRecordRequest request)
        {
            _timeSpaceManager.TryAddNewRecord(request.TimeSpaceRecordDto);
            return new EmptyResponse();
        }

        public async ValueTask<TimeSpaceRecordResponse> GetTimeSpaceRecord(TimeSpaceRecordRequest request) =>
            new TimeSpaceRecordResponse
            {
                TimeSpaceRecordDto = await _timeSpaceManager.GetRecordByTimeSpaceId(request.TimeSpaceId)
            };
    }
}