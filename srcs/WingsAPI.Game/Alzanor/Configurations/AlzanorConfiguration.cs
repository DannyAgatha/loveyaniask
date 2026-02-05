using System;
using System.Collections.Generic;

namespace WingsEmu.Game.Alzanor.Configurations;

public class AlzanorConfiguration
{
    public List<TimeSpan> Warnings { get; init; }
    public bool SendMessageOnKill { get; init; }
    public short MinimumPlayers { get; init; }
    public short MaximumPlayers { get; init; }
    public short DurationInMinutes { get; init; }
    public short SecondsBeingDead { get; init; }
    public short PointsPerKill { get; init; }
    public short MinusPointsPerDeath { get; init; }
    public Rewards[] WinRewards { get; init; }
    public Rewards[] LoseRewards { get; init; }
    public bool GiveKillReputationReward { get; init; }
    public short KillRewardReputationMultiplier { get; init; }
    public TopPlayerRewards[] TopPlayerRewards { get; init; }
    public int MapId { get; init; }
    public short RedStartX { get; init; }
    public short RedEndX { get; init; }
    public short RedStartY { get; init; }
    public short RedEndY { get; init; }
    public short BlueStartX { get; init; }
    public short BlueEndX { get; init; }
    public short BlueStartY { get; init; }
    public short BlueEndY { get; init; }
    public short BossX { get; init; }
    public short BossY { get; init; }
}

public class Rewards
{
    public int ItemId { get; set; }
    public short Amount { get; set; }
    public int Reputation { get; set; }
}

public class TopPlayerRewards
{
    public short Position { get; set; }
    public int ItemId { get; set; }
    public short Amount { get; set; }
    public int Reputation { get; set; }
}