using ProtoBuf;

namespace WingsAPI.Communication.Sessions.Request
{
    [ProtoContract]
    public class ConnectToWorldServerRequest
    {
        [ProtoMember(1)]
        public long AccountId { get; init; }

        [ProtoMember(2)]
        public string ServerGroup { get; init; }

        [ProtoMember(3)]
        public int ChannelId { get; init; }
    }
}