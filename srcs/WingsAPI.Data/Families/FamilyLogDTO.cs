// NosEmu
// 


using System;
using PhoenixLib.DAL;
using ProtoBuf;
using WingsEmu.Packets.Enums.Families;

namespace WingsAPI.Data.Families;

[ProtoContract]
public class FamilyLogDto : ILongDto
{
    [ProtoMember(2)]
    public long FamilyId { get; set; }

    [ProtoMember(3)]
    public FamilyLogType FamilyLogType { get; set; }

    [ProtoMember(4)]
    public DateTime Timestamp { get; set; }

    [ProtoMember(5)]
    public string Actor { get; set; }

    [ProtoMember(6)]
    public string Argument1 { get; set; }

    [ProtoMember(7)]
    public string Argument2 { get; set; }

    [ProtoMember(8)]
    public string Argument3 { get; set; }

    [ProtoMember(1)]
    public long Id { get; set; }
}