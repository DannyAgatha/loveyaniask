using ProtoBuf;

namespace WingsAPI.Communication.Families
{
    [ProtoContract]
    public class FamilyInvitationContainsResponse
    {
        [ProtoMember(1)]
        public bool IsContains { get; set; }
    }
}