using System.Threading.Tasks;
using WingsAPI.Data.TimeSpace;

namespace DatabaseServer.Managers
{
    public interface ITimeSpaceManager
    {
        Task<TimeSpaceRecordDto> GetRecordByTimeSpaceId(long tsId);
        Task FlushTimeSpaceRecords();
        Task Initialize();
        void TryAddNewRecord(TimeSpaceRecordDto record);
        Task<bool> IsNewRecord(long tsId, long points);
    }
}