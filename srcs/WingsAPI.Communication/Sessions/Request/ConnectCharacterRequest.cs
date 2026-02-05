using ProtoBuf;

namespace WingsAPI.Communication.Sessions.Request
{
    [ProtoContract]
    public class ConnectCharacterRequest
    {
        [ProtoMember(1)]
        public long AccountId { get; init; }

        [ProtoMember(2)]
        public int ChannelId { get; init; }

        [ProtoMember(3)]
        public long CharacterId { get; init; }
    }
}