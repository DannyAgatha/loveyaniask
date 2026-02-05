using ProtoBuf;

namespace WingsAPI.Communication.Families
{
    [ProtoContract]
    public class MembershipRequest
    {
        [ProtoMember(1)]
        public long CharacterId { get; set; }
    }
}