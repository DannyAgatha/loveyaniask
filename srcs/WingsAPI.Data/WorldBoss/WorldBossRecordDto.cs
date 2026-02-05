using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using WingsAPI.Packets.Enums.WorldBoss;

namespace WingsAPI.Data.WorldBoss;

[ProtoContract]
public class WorldBossRecordDto
{
    [ProtoMember(1)]
    public int BossVnum { get; set; }

    [ProtoMember(2)]
    public WorldBossModeType Mode { get; set; }

    [ProtoMember(3)]
    public DateTime Timestamp { get; set; }

    [ProtoMember(4)]
    public bool Success { get; set; }
    
    [ProtoMember(5)]
    public double ProgressPercentage { get; set; } 
    
    [ProtoMember(6)]
    public long TicketsAcquired { get; set; }

    [ProtoMember(7)]
    public long TicketsTotal { get; set; }

    [ProtoMember(8)]
    public double FinalWinRate { get; set; }
    
    [ProtoMember(9)]
    public bool WonLottery { get; set; }
    
    [ProtoMember(10)]
    public bool HasGuaranteed { get; set; }
    
    [ProtoMember(11)]
    public bool PendingReward { get; set; }
    
    [ProtoMember(12)]
    public Dictionary<string, bool> WonRewards { get; set; } = new(); 
    
    [ProtoMember(13)]
    public Dictionary<string, double> RewardBoosts { get; set; } = new();
    
    [ProtoMember(14)]
    public int MyParticipationCount { get; set; }

    [ProtoMember(15)]
    public int TotalParticipationCount { get; set; }
    
    [ProtoMember(16)]
    public Dictionary<string, double> LotteryWinRates { get; set; } = new();
}