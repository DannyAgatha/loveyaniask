using ProtoBuf;

namespace WingsAPI.Communication.Families
{
    [ProtoContract]
    public class FamilyDisbandRequest
    {
        [ProtoMember(1)]
        public long FamilyId { get; set; }
    }
}