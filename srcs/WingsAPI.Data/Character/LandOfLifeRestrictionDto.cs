using System;
using ProtoBuf;

namespace WingsAPI.Data.Character;

[ProtoContract]
public class LandOfLifeRestrictionDto
{
    [ProtoMember(1)]
    public int SecondsUsedToday { get; set; }
    
    [ProtoMember(2)]
    public DateTime LastResetDate { get; set; }
}