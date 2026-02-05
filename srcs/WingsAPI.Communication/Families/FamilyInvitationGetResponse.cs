using ProtoBuf;

namespace WingsAPI.Communication.Families
{
    [ProtoContract]
    public class FamilyInvitationGetResponse
    {
        [ProtoMember(1)]
        public FamilyInvitation Invitation { get; set; }
    }
}