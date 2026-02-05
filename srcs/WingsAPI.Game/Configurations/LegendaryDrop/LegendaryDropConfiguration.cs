using System.Collections.Generic;

namespace WingsEmu.Game.Configurations.LegendaryDrop
{
    public class LegendaryDropConfiguration
    {
        public List<LegendaryItemGroup> LegendaryItems { get; set; } = [];
    }

    public class LegendaryItemGroup
    {
        public List<int> ItemVnums { get; set; } = [];
    }
}