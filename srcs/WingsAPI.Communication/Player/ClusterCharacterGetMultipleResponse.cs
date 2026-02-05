using System.Collections.Generic;
using ProtoBuf;

namespace WingsAPI.Communication.Player
{
    [ProtoContract]
    public class ClusterCharacterGetMultipleResponse
    {
        [ProtoMember(1)]
        public RpcResponseType ResponseType { get; init; }

        [ProtoMember(2)]
        public IReadOnlyList<ClusterCharacterInfo> ClusterCharacterInfo { get; init; }
    }
}