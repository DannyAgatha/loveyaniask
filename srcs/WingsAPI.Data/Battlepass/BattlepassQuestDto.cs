using ProtoBuf;
using System;
using WingsAPI.Packets.Enums.BattlePass;

namespace WingsAPI.Data.BattlePass
{
    [ProtoContract]
    public class BattlePassQuestDto
    {
        [ProtoMember(1)]
        public long QuestId { get; set; }

        [ProtoMember(2)]
        public long Advancement { get; set; }

        [ProtoMember(3)]
        public bool RewardAlreadyTaken { get; set; }

        [ProtoMember(4)]
        public FrequencyType FrequencyType { get; set; }

        [ProtoMember(5)]
        public DateTime AccomplishedDate { get; set; }
    }
}
