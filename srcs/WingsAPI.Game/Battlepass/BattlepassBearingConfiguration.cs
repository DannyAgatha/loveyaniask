using System.Collections.Generic;

namespace WingsEmu.Game.BattlePass
{
    public class BattlePassBearingConfiguration
    {
        public List<BattlepassBearing> Bearings { get; set; }
    }

    public class BattlepassBearing
    {
        public long Id { get; set; }

        public int MinimumBattlepassPoint { get; set; }

        public int MaximumBattlepassPoint { get; set; }
    }
}
