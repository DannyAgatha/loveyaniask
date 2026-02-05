using ProtoBuf;

namespace WingsAPI.Communication.Families
{
    [ProtoContract]
    public class MembershipTodayResponse
    {
        [ProtoMember(1)]
        public bool CanPerformAction { get; set; }
    }
}