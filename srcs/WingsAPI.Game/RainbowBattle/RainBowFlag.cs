using System;
using WingsAPI.Packets.Enums.Rainbow;

namespace WingsEmu.Game.RainbowBattle;

public class RainBowFlag
{
    public RainbowBattleFlagTeamType FlagTeamType { get; set; }
    public RainbowBattleFlagType FlagType { get; set; }
    public DateTime RainbowBattleLastTakeOver { get; set; }
}