using System.Collections.Generic;
using System.Threading.Tasks;

namespace WingsAPI.Data.TimeSpace;

public interface ITimeSpaceRecordDao
{
    Task<TimeSpaceRecordDto> GetRecordById(long timeSpaceId);
    Task SaveRecord(TimeSpaceRecordDto recordDto);
    Task<IEnumerable<TimeSpaceRecordDto>> GetAllRecords();
}