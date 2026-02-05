using System;
using PhoenixLib.DAL;
using ProtoBuf;
using WingsEmu.Packets.Enums;

namespace WingsAPI.Data.Account;

[ProtoContract]
public class AccountPenaltyDto : ILongDto
{
    [ProtoMember(2)]
    public long AccountId { get; set; }

    [ProtoMember(3)]
    public string JudgeName { get; set; }

    [ProtoMember(4)]
    public string TargetName { get; set; }

    [ProtoMember(5)]
    public DateTime Start { get; set; }

    [ProtoMember(6)]
    public int? RemainingTime { get; set; }

    [ProtoMember(7)]
    public PenaltyType PenaltyType { get; set; }

    [ProtoMember(8)]
    public string Reason { get; set; }

    [ProtoMember(9)]
    public string UnlockReason { get; set; }

    [ProtoMember(1)]
    public long Id { get; set; }
}