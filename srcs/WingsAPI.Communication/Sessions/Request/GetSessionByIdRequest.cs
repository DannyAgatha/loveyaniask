using ProtoBuf;

namespace WingsAPI.Communication.Sessions.Request
{
    [ProtoContract]
    public class GetSessionByIdRequest
    {
        [ProtoMember(1)]
        public string SessionId { get; init; }
    }
}