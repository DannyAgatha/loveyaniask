using ProtoBuf;

namespace WingsAPI.Communication.Families
{
    [ProtoContract]
    public class FamilyRemoveMemberByCharIdRequest
    {
        [ProtoMember(1)]
        public long CharacterId { get; init; }
    }
}