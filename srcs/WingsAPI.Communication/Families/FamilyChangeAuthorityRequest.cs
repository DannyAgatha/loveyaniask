using System.Collections.Generic;
using ProtoBuf;

namespace WingsAPI.Communication.Families
{
    [ProtoContract]
    public class FamilyChangeAuthorityRequest
    {
        [ProtoMember(1)]
        public List<FamilyChangeContainer> FamilyMembers { get; set; }
    }
}