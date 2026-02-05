using System;
using ProtoBuf;

namespace WingsAPI.Data.Families;

[ProtoContract]
public class FamilyWarehouseLogEntryDto
{
    [ProtoMember(1)]
    public long CharacterId { get; set; }

    [ProtoMember(2)]
    public DateTime DateOfLog { get; set; }

    [ProtoMember(3)]
    public FamilyWarehouseLogEntryType Type { get; set; }

    [ProtoMember(4)]
    public int ItemVnum { get; set; }

    [ProtoMember(5)]
    public int Amount { get; set; }

    [ProtoMember(6)]
    public string CharacterName { get; set; }
}

public enum FamilyWarehouseLogEntryType
{
    List = 0,
    Withdraw = 1
}