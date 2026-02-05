using System.Collections.Generic;
using ProtoBuf;
using WingsAPI.Data.Character;

namespace WingsAPI.Communication.DbServer.CharacterService
{
    [ProtoContract]
    public class CharacterGetTopResponse
    {
        [ProtoMember(1)]
        public RpcResponseType ResponseType { get; init; }

        [ProtoMember(2)]
        public IReadOnlyList<CharacterDTO> Top { get; init; }
    }
}