using System;
using System.Collections.Generic;

namespace WingsEmu.Game.Configurations.UnderWaterShowdown;

public class UnderWaterShowdownConfiguration
{
    public int MapId { get; init; }
    
    public int MonsterVnum { get; init; }
    
    public short MonsterPosX { get; init; }
    
    public short MonsterPosY { get; init; }
    
    public short PosXStartLine { get; init; }
    
    public short PosYStartLine { get; init; }
    
    public short SpaceBetweenPlayers { get; init; }
    
    public short PlayersPerLine { get; init; }
    
    public short MinAmountOfPlayers { get; init; }
    
    public short MaxAmountOfPlayers { get; init; }
    
    public TimeSpan EventDuration { get; init; }

    public short DurationAfkInSeconds { get; init; }
    
    public int RoundDurationInSeconds { get; init; }
    
    public int CantMoveDurationInSeconds { get; init; }

    public List<TimeSpan> Warnings { get; init; }
    
    public UnderWaterShowdownReward Rewards { get; init; }
    
    public List<UnderWaterShowdownRound> Rounds { get; init; }
}

public class UnderWaterShowdownReward
{
    public short NormalLevelXpPercentage { get; init; }
    
    public short HeroLevelXpPercentage { get; init; }
    
    public int Reputation { get; init; }
    
    public int Gold { get; init; }
    
    public List<UnderWaterShowdownRewardItem> Items { get; init; }
}

public class UnderWaterShowdownRound
{
    public List<UnderWaterShowdownRewardItem> Drops { get; init; }
}

public class UnderWaterShowdownRewardItem
{
    public int ItemVnum { get; init; }
    
    public int Amount { get; init; }
    
    public short? BunchCount { get; init; }
}