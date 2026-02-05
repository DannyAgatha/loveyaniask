using System;
using ProtoBuf;
using WingsAPI.Packets.Enums;

namespace WingsAPI.Data.Character
{
    [ProtoContract]
    public class CharacterPityDto
    {
        [ProtoMember(1)]
        public long ItemVnum { get; set; }
        
        [ProtoMember(2)]
        public byte Amount { get; set; }
        
        [ProtoMember(3)]
        public Guid ItemGuidTracked { get; set; }
        
        [ProtoMember(4)]
        public PityType PityType { get; set; }
        
        [ProtoMember(5)]
        public int PityCounter { get; set; }
        
        [ProtoMember(6)]
        public long GoldSpent { get; set; }
    }
}