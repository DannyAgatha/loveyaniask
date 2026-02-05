using ProtoBuf;

namespace WingsAPI.Communication.Sessions.Request
{
    [ProtoContract]
    public class GetSessionByAccountNameRequest
    {
        [ProtoMember(1)]
        public string AccountName { get; init; }
    }
}