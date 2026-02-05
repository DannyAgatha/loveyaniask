using ProtoBuf;

namespace WingsAPI.Communication.Families
{
    [ProtoContract]
    public class FamilyInvitationSaveRequest
    {
        [ProtoMember(1)]
        public FamilyInvitation Invitation { get; set; }
    }
}