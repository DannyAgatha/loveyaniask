using ProtoBuf;

namespace WingsAPI.Communication.DbServer.TimeSpaceService
{
    [ProtoContract]
    public class TimeSpaceRecordRequest
    {
        [ProtoMember(1)]
        public long TimeSpaceId { get; init; }
    }
}