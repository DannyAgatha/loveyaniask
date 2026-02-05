using ProtoBuf;

namespace WingsAPI.Communication.Families
{
    [ProtoContract]
    public class FamilyChangeFactionResponse
    {
        [ProtoMember(1)]
        public FamilyChangeFactionResponseType Status { get; set; }
    }
}