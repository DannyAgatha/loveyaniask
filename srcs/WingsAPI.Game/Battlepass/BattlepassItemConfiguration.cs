using System.Collections.Generic;

namespace WingsEmu.Game.BattlePass
{
    public class BattlePassItemConfiguration
    {
        public List<BattlepassItem> Items { get; set; }
    }

    public class BattlepassItem
    {
        public long Id { get; set; }

        public short ItemVnum { get; set; }

        public int Amount { get; set; }

        public bool IsSuperReward { get; set; }

        public bool IsPremium { get; set; }

        public long BearingId { get; set; }
    }
}
