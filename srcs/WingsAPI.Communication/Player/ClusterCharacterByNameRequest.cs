using ProtoBuf;

namespace WingsAPI.Communication.Player
{
    [ProtoContract]
    public class ClusterCharacterByNameRequest
    {
        [ProtoMember(1)]
        public string CharacterName { get; init; }
    }
}