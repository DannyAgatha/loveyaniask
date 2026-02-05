using ProtoBuf;
using WingsAPI.Data.Families;

namespace WingsAPI.Communication.Families
{
    [ProtoContract]
    public class FamilyAddMemberResponse
    {
        [ProtoMember(1)]
        public FamilyMembershipDto Member { get; set; }
    }
}