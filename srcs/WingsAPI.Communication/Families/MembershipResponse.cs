using ProtoBuf;
using WingsAPI.Data.Families;

namespace WingsAPI.Communication.Families
{
    [ProtoContract]
    public class MembershipResponse
    {
        [ProtoMember(1)]
        public FamilyMembershipDto Membership { get; set; }
    }
}