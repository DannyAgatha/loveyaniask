using ProtoBuf;

namespace WingsAPI.Communication.DbServer.TimeSpaceService
{
    [ProtoContract]
    public class TimeSpaceIsNewRecordRequest
    {
        [ProtoMember(1)]
        public long TimeSpaceId { get; init; }

        [ProtoMember(2)]
        public long Record { get; init; }
    }
}