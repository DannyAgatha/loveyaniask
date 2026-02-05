using ProtoBuf;
using WingsEmu.DTOs.Relations;

namespace WingsAPI.Communication.Relation
{
    [ProtoContract]
    public class RelationAddResponse
    {
        [ProtoMember(1)]
        public CharacterRelationDTO SenderRelation { get; set; }

        [ProtoMember(2)]
        public CharacterRelationDTO TargetRelation { get; set; }

        [ProtoMember(3)]
        public RpcResponseType ResponseType { get; set; }
    }
}