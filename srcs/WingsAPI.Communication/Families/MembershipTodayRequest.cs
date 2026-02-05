using ProtoBuf;

namespace WingsAPI.Communication.Families
{
    [ProtoContract]
    public class MembershipTodayRequest
    {
        [ProtoMember(1)]
        public long CharacterId { get; set; }

        [ProtoMember(2)]
        public string CharacterName { get; set; }
    }
}