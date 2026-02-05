using ProtoBuf;

namespace WingsAPI.Communication.Families
{
    [ProtoContract]
    public class FamilyUpgradeResponse
    {
        [ProtoMember(1)]
        public FamilyUpgradeAddResponseType ResponseType { get; set; }
    }
}