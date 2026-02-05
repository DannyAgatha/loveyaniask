using System;
using WingsAPI.Packets.Enums.Rainbow;

namespace WingsEmu.Game.RainbowBattle;

public interface IRainbowBattleComponent
{
    int Kills { get; set; }
    int Deaths { get; set; }
    int KillStreak { get; set; }
    bool IsInKillStreak { get; set; }
    bool IsInRainbowBattle { get; }
    bool IsFrozen { get; set; }
    DateTime? FrozenTime { get; set; }
    RainbowBattleParty RainbowBattleParty { get; }
    int ActivityPoints { get; set; }
    int CapturedFlagCount  { get; set; }
    int UnfrozenPlayerCount  { get; set; }
    RainbowBattleTeamType Team { get; }
    void SetRainbowBattle(RainbowBattleParty rainbowBattleParty, RainbowBattleTeamType team);
    void RemoveRainbowBattle();
    bool IsBattleEnded { get; set; }
}

public class RainbowBattleComponent : IRainbowBattleComponent
{
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int KillStreak { get; set; }
    public bool IsInKillStreak { get; set; }
    public bool IsInRainbowBattle => RainbowBattleParty != null;
    public bool IsFrozen { get; set; }
    public DateTime? FrozenTime { get; set; }
    public RainbowBattleParty RainbowBattleParty { get; private set; }
    public int ActivityPoints { get; set; }
    public int CapturedFlagCount  { get; set; }
    public int UnfrozenPlayerCount  { get; set; }
    public RainbowBattleTeamType Team { get; private set; }
    
    public bool IsBattleEnded { get; set; } = false;

    public void SetRainbowBattle(RainbowBattleParty rainbowBattleParty, RainbowBattleTeamType team)
    {
        RainbowBattleParty = rainbowBattleParty;
        Team = team;
        Kills = 0;
        Deaths = 0;
        KillStreak = 0;
        IsInKillStreak = false;
        IsFrozen = false;
        ActivityPoints = 0;
        CapturedFlagCount = 0;
        UnfrozenPlayerCount = 0;
        IsBattleEnded = false;
    }

    public void RemoveRainbowBattle()
    {
        RainbowBattleParty = null;
        Kills = 0;
        Deaths = 0;
        KillStreak = 0;
        IsInKillStreak = false;
        IsFrozen = false;
        ActivityPoints = 0;
        CapturedFlagCount = 0;
        UnfrozenPlayerCount = 0;
        IsBattleEnded = false;
    }
}