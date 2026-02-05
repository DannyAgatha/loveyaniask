using ProtoBuf;

namespace WingsAPI.Communication.Families
{
    [ProtoContract]
    public class FamilyInvitationRemoveRequest
    {
        [ProtoMember(1)]
        public long SenderId { get; set; }
    }
}