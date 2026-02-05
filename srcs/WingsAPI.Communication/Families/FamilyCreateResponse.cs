using ProtoBuf;
using WingsAPI.Data.Families;

namespace WingsAPI.Communication.Families
{
    [ProtoContract]
    public class FamilyCreateResponse
    {
        [ProtoMember(1)]
        public FamilyCreateResponseType Status { get; set; }

        [ProtoMember(2)]
        public FamilyDTO Family { get; set; }
    }
}