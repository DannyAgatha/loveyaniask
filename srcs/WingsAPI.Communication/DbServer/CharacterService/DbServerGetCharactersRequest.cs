using ProtoBuf;

namespace WingsAPI.Communication.DbServer.CharacterService
{
    [ProtoContract]
    public class DbServerGetCharactersRequest
    {
        [ProtoMember(1)]
        public long AccountId { get; set; }
    }
}