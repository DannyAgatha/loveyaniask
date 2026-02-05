using System.Collections.Generic;

namespace WingsEmu.Game.Configurations
{
    public class ItemBuyDailyLimitConfiguration
    {
        public List<ItemBuyDailyLimit> Items { get; set; }
    }

    public class ItemBuyDailyLimit
    {
        public int ItemVnum { get; set; }
        public int Amount { get; set; }
    }
}