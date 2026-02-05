using ProtoBuf;

namespace WingsAPI.Communication.Player
{
    [ProtoContract]
    public class ClusterCharacterByChannelIdRequest
    {
        [ProtoMember(1)]
        public byte ChannelId { get; init; }
    }
}