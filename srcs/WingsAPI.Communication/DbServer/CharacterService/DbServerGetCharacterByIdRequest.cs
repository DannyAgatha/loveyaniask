using ProtoBuf;

namespace WingsAPI.Communication.DbServer.CharacterService
{
    [ProtoContract]
    public class DbServerGetCharacterByIdRequest
    {
        [ProtoMember(1)]
        public long CharacterId { get; set; }
    }
}