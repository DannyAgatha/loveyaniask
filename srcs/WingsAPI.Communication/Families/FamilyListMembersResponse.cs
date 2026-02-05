using System.Collections.Generic;
using ProtoBuf;
using WingsAPI.Data.Families;

namespace WingsAPI.Communication.Families
{
    [ProtoContract]
    public class FamilyListMembersResponse
    {
        [ProtoMember(1)]
        public List<FamilyMembershipDto> Members { get; set; }
    }
}