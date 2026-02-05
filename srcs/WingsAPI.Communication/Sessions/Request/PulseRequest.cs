using ProtoBuf;

namespace WingsAPI.Communication.Sessions.Request
{
    [ProtoContract]
    public class PulseRequest
    {
        [ProtoMember(1)]
        public long AccountId { get; init; }
    }
}