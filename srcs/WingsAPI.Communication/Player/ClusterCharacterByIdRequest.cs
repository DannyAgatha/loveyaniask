using ProtoBuf;

namespace WingsAPI.Communication.Player
{
    [ProtoContract]
    public class ClusterCharacterByIdRequest
    {
        [ProtoMember(1)]
        public long CharacterId { get; init; }
    }
}