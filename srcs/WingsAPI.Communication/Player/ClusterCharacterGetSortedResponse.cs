using System.Collections.Generic;
using ProtoBuf;

namespace WingsAPI.Communication.Player
{
    [ProtoContract]
    public class ClusterCharacterGetSortedResponse
    {
        [ProtoMember(1)]
        public RpcResponseType ResponseType { get; init; }

        [ProtoMember(2)]
        public IReadOnlyCollection<KeyValuePair<byte, List<ClusterCharacterInfo>>> CharactersByChannel { get; init; }
    }
}