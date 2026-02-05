using System.Collections.Generic;
using ProtoBuf;
using WingsAPI.Data.Families;

namespace WingsAPI.Communication.Families
{
    [ProtoContract]
    public class FamilyIdResponse
    {
        [ProtoMember(1)]
        public FamilyDTO Family { get; set; }

        [ProtoMember(2)]
        public List<FamilyMembershipDto> Members { get; set; }

        [ProtoMember(3)]
        public List<FamilyLogDto> Logs { get; set; }
    }
}