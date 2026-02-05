using System.Collections.Generic;
using ProtoBuf;
using WingsEmu.DTOs.Relations;

namespace WingsAPI.Communication.Relation
{
    [ProtoContract]
    public class RelationGetAllResponse
    {
        [ProtoMember(1)]
        public RpcResponseType ResponseType { get; init; }

        [ProtoMember(2)]
        public IReadOnlyList<CharacterRelationDTO> CharacterRelationDtos { get; init; }
    }
}