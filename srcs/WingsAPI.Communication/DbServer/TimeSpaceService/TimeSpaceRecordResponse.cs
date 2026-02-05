using ProtoBuf;
using WingsAPI.Data.TimeSpace;

namespace WingsAPI.Communication.DbServer.TimeSpaceService
{
    [ProtoContract]
    public class TimeSpaceRecordResponse
    {
        [ProtoMember(1)]
        public TimeSpaceRecordDto TimeSpaceRecordDto { get; init; }
    }
}