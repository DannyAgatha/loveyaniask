using ProtoBuf;
using WingsAPI.Data.Character;

namespace WingsAPI.Communication.DbServer.CharacterService
{
    [ProtoContract]
    public class DbServerDeleteCharacterRequest
    {
        [ProtoMember(1)]
        public CharacterDTO CharacterDto { get; set; }
    }
}