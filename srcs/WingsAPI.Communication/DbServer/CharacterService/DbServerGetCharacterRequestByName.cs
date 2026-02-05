using ProtoBuf;

namespace WingsAPI.Communication.DbServer.CharacterService
{
    [ProtoContract]
    public class DbServerGetCharacterRequestByName
    {
        [ProtoMember(1)]
        public string CharacterName { get; set; }
    }
}