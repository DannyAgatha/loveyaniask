using System;

namespace WingsEmu.Game.BattlePass
{
    public class BattlePassConfiguration
    {
        public byte BattlePassSeason { get; set; }

        public int MaxBattlePassPoints { get; set; }

        public bool IsBattlePassEnabled { get; set; }

        public DateTime StartSeason { get; set; }

        public DateTime EndSeason { get; set; }
    }
}