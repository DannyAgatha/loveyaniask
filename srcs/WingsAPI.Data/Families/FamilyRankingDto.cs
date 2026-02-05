using ProtoBuf;

namespace WingsAPI.Data.Families
{
    [ProtoContract]
    public class FamilyRankingDto
    {
        [ProtoMember(1)]
        public long ExpEarned { get; set; }

        [ProtoMember(2)]
        public long PvpPoints { get; set; }
        
        [ProtoMember(3)]
        public long RainbowBattlePoints { get; set; }
    }
}