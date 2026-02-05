using ProtoBuf;

namespace WingsAPI.Communication.DbServer.CharacterService
{
    [ProtoContract]
    public class DbServerGetCharacterFromSlotRequest
    {
        [ProtoMember(1)]
        public long AccountId { get; set; }

        [ProtoMember(2)]
        public byte Slot { get; set; }
    }
}