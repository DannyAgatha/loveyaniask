using System;
using ProtoBuf;
using WingsEmu.Packets.Enums;

namespace WingsAPI.Data.Families;

[ProtoContract]
public class FamilyBuffCrossChannel
{
    [ProtoMember(1)]
    public long FamilyId { get; set; }
    
    [ProtoMember(2)]
    public string FamilyName { get; set; }
    
    [ProtoMember(3)]
    public int BuffVnum { get; set; }
    
    [ProtoMember(4)]
    public int ItemVnum { get; set; }
    
    [ProtoMember(5)]
    public FactionType? FactionType { get; set; }
    
    [ProtoMember(6)]
    public DateTime EndTime { get; set; }
}