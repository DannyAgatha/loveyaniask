using System.Collections.Generic;

namespace WingsEmu.Game.Configurations.Act7.Tattoos;

public class CraftTattooItemsConfiguration
{
    public int Gold { get; set; }
    public List<TattooItem> Items { get; set; }
}

public class TattooItem
{
    public int Vnum { get; set; }
    public short Quantity { get; set; }
}