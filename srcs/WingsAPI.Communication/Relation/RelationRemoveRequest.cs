using ProtoBuf;
using WingsEmu.Packets.Enums.Relations;

namespace WingsAPI.Communication.Relation
{
    [ProtoContract]
    public class RelationRemoveRequest
    {
        [ProtoMember(1)]
        public long CharacterId { get; set; }

        [ProtoMember(2)]
        public long TargetId { get; set; }

        [ProtoMember(3)]
        public CharacterRelationType RelationType { get; set; }
    }
}