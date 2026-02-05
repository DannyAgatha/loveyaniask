using ProtoBuf;

namespace WingsAPI.Communication.DbServer.TimeSpaceService
{
    [ProtoContract]
    public class TimeSpaceIsNewRecordResponse
    {
        [ProtoMember(1)]
        public bool IsNewRecord { get; init; }
    }
}