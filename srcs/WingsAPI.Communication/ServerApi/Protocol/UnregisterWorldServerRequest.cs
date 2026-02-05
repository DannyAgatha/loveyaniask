using ProtoBuf;

namespace WingsAPI.Communication.ServerApi.Protocol
{
    [ProtoContract]
    public class UnregisterWorldServerRequest
    {
        [ProtoMember(1)]
        public int ChannelId { get; init; }
    }
}