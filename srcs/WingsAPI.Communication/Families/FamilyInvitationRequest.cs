using ProtoBuf;

namespace WingsAPI.Communication.Families
{
    [ProtoContract]
    public class FamilyInvitationRequest
    {
        [ProtoMember(1)]
        public long SenderId { get; set; }

        [ProtoMember(2)]
        public long TargetId { get; set; }
    }
}