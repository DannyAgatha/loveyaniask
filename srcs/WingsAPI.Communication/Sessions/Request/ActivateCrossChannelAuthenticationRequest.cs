using ProtoBuf;

namespace WingsAPI.Communication.Sessions.Request
{
    [ProtoContract]
    public class ActivateCrossChannelAuthenticationRequest
    {
        [ProtoMember(1)]
        public long AccountId { get; init; }

        [ProtoMember(2)]
        public long ChannelId { get; init; }
    }
}