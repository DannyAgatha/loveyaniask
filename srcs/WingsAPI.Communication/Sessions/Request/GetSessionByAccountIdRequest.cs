using ProtoBuf;

namespace WingsAPI.Communication.Sessions.Request
{
    [ProtoContract]
    public class GetSessionByAccountIdRequest
    {
        [ProtoMember(1)]
        public long AccountId { get; init; }
    }
}