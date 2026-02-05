using ProtoBuf;

namespace WingsAPI.Communication.ServerApi.Protocol
{
    [ProtoContract]
    public class GetChannelInfoRequest
    {
        [ProtoMember(1)]
        public string WorldGroup { get; init; }

        [ProtoMember(2)]
        public int ChannelId { get; init; }
    }
}