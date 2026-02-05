using ProtoBuf;

namespace WingsAPI.Communication.Families
{
    [ProtoContract]
    public class FamilyInvitation
    {
        [ProtoMember(1)]
        public long SenderId { get; set; }

        [ProtoMember(2)]
        public long SenderFamilyId { get; set; }

        [ProtoMember(3)]
        public long TargetId { get; set; }
    }
}