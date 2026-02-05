using ProtoBuf;
using WingsAPI.Data.TimeSpace;

namespace WingsAPI.Communication.DbServer.TimeSpaceService
{
    [ProtoContract]
    public class TimeSpaceNewRecordRequest
    {
        [ProtoMember(1)]
        public TimeSpaceRecordDto TimeSpaceRecordDto { get; init; }
    }
}