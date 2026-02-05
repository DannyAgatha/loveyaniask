using System.Collections.Generic;
using ProtoBuf;
using WingsAPI.Data.Character;

namespace WingsAPI.Communication.DbServer.CharacterService
{
    [ProtoContract]
    public class DbServerGetCharactersResponse
    {
        [ProtoMember(1)]
        public RpcResponseType RpcResponseType { get; set; }

        [ProtoMember(2)]
        public IEnumerable<CharacterDTO> Characters { get; set; }
    }
}