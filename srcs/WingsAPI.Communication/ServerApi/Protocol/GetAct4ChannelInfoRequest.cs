using ProtoBuf;

namespace WingsAPI.Communication.ServerApi.Protocol
{
    [ProtoContract]
    public class GetAct4ChannelInfoRequest
    {
        [ProtoMember(1)]
        public string WorldGroup { get; init; }
    }
}