using ProtoBuf;

namespace WingsAPI.Communication.Player
{
    [ProtoContract]
    public class ClusterCharacterResponse
    {
        [ProtoMember(1)]
        public RpcResponseType ResponseType { get; init; }

        [ProtoMember(2)]
        public ClusterCharacterInfo ClusterCharacterInfo { get; init; }
    }
}