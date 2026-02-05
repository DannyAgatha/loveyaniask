using System.Collections.Generic;
using ProtoBuf;
using WingsAPI.Data.Families;

namespace WingsAPI.Communication.Families
{
    [ProtoContract]
    public class FamilyCreateRequest
    {
        [ProtoMember(1)]
        public string Name { get; set; }

        [ProtoMember(2)]
        public byte Level { get; set; }

        [ProtoMember(3)]
        public byte MembershipCapacity { get; set; }

        [ProtoMember(4)]
        public byte Faction { get; set; }

        [ProtoMember(5)]
        public List<FamilyMembershipDto> Members { get; set; }
    }
}