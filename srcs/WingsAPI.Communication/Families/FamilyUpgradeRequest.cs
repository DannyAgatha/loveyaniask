using ProtoBuf;
using WingsAPI.Data.Families;

namespace WingsAPI.Communication.Families
{
    [ProtoContract]
    public class FamilyUpgradeRequest
    {
        [ProtoMember(1)]
        public long FamilyId { get; set; }

        [ProtoMember(2)]
        public int UpgradeId { get; set; }

        [ProtoMember(3)]
        public FamilyUpgradeType FamilyUpgradeType { get; set; }

        [ProtoMember(4)]
        public short Value { get; set; }

        [ProtoMember(5)]
        public short? SecondValue{ get; set; }
    }
}