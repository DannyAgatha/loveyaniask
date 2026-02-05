using ProtoBuf;
using WingsAPI.Data.Families;

namespace WingsAPI.Communication.Families
{
    [ProtoContract]
    public class FamilyAddMemberRequest
    {
        [ProtoMember(1)]
        public FamilyMembershipDto Member { get; set; }

        [ProtoMember(2)]
        public string Nickname { get; set; }

        [ProtoMember(3)]
        public long SenderId { get; set; }
    }
}