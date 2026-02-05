using ProtoBuf;
using WingsAPI.Data.Character;

namespace WingsAPI.Communication.DbServer.CharacterService
{
    [ProtoContract]
    public class DbServerGetCharacterResponse
    {
        [ProtoMember(1)]
        public RpcResponseType RpcResponseType { get; set; }

        [ProtoMember(2)]
        public CharacterDTO CharacterDto { get; set; }
    }
}