using ProtoBuf;

namespace WingsAPI.Data.Character
{
    [ProtoContract]
    public class BattlePassOptionDto
    {
        [ProtoMember(1)]
        public bool HavePremium { get; set; }

        [ProtoMember(2)]
        public long Points { get; set; }

        [ProtoMember(3)]
        public long Jewels { get; set; }
    }
}