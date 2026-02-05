using ProtoBuf;
using WingsAPI.Data.Character;

namespace WingsAPI.Communication.DbServer.CharacterService
{
    [ProtoContract]
    public class DbServerSaveCharacterRequest
    {
        [ProtoMember(1)]
        public CharacterDTO Character { get; set; }

        [ProtoMember(2)]
        public bool IgnoreSlotCheck { get; set; }
    }
}