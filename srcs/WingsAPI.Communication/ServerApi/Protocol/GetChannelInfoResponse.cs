using ProtoBuf;

namespace WingsAPI.Communication.ServerApi.Protocol
{
    [ProtoContract]
    public class GetChannelInfoResponse
    {
        [ProtoMember(1)]
        public RpcResponseType ResponseType { get; init; }

        [ProtoMember(2)]
        public SerializableGameServer GameServer { get; init; }
    }
}