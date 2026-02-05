// NosEmu
// 


using System.Collections.Generic;
using ProtoBuf;
using WingsAPI.Data.Character;
using WingsEmu.Packets.Enums.Character;

namespace WingsAPI.Communication.Player
{
    [ProtoContract]
    public class ClusterCharacterInfo
    {
        [ProtoMember(1)]
        public long Id { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }

        [ProtoMember(3)]
        public GenderType Gender { get; set; }

        [ProtoMember(4)]
        public ClassType Class { get; set; }

        [ProtoMember(5)]
        public byte Level { get; set; }

        [ProtoMember(6)]
        public byte HeroLevel { get; set; }

        [ProtoMember(7)]
        public int? MorphId { get; set; }

        [ProtoMember(8)]
        public byte? ChannelId { get; set; }

        [ProtoMember(9)]
        public string HardwareId { get; set; }
        
        [ProtoMember(10)]
        public SubClassType SubClass{ get; set; }
        
        [ProtoMember(11)]
        public byte TierLevel { get; set; }
        
        [ProtoMember(12)]
        public byte PrestigeLevel { get; set; }
    }
}