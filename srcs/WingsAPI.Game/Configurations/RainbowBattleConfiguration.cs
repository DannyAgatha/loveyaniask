using System;
using System.Collections.Generic;
using WingsAPI.Packets.Enums.Rainbow;
using WingsEmu.Core;
using WingsEmu.Game._enum;

namespace WingsEmu.Game.Configurations;

public class RainbowBattleConfiguration
{
    public int MapId { get; init; }
    public List<TimeSpan> Warnings { get; init; }
    public int MinimumPlayers { get; init; }
    public int MaximumPlayers { get; init; }
    
    public short SecondsBeingFrozen { get; init; }
    public short DelayBetweenCapture { get; init; }
    public short RedStartX { get; init; }
    public short RedEndX { get; init; }
    public short BlueStartX { get; init; }
    public short BlueEndX { get; init; }
    public short RedStartY { get; init; }
    public short RedEndY { get; init; }
    public short BlueStartY { get; init; }
    public short BlueEndY { get; init; }
    public short UnfreezeActivityPoints { get; init; }
    public short CaptureActivityPoints { get; init; }
    public short UsingSkillActivityPoints { get; init; }
    public short NeededActivityPoints { get; init; }
    public short KillActivityPoints { get; init; }
    public short DeathActivityPoints { get; init; }
    public short WalkingActivityPoints { get; init; }
    public List<Range<byte>> LevelRange { get; init; }
    public List<FlagPosition> MainFlags { get; init; }
    public List<FlagPosition> MediumFlags { get; init; }
    public List<FlagPosition> SmallFlags { get; init; }
    public int ReputationMultiplier { get; init; }
    public int RequiredKillsForStreak { get; init; }
}

public class FlagPosition
{
    public short X { get; init; }
    public short Y { get; init; }
}