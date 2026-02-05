using System.Collections.Generic;

namespace NosEmu.Plugins.BasicImplementations.Event.Items;

public class GamblingRarityInfo
{
    public List<RaritySuccess> GamblingSuccess { get; set; }
    public List<RarityChance> GamblingRarities { get; set; }
    public short GoldPrice { get; set; }
    public byte CellaUsed { get; set; }
}

public class RarityChance
{
    public sbyte Rarity { get; set; }
    public int Chance { get; set; }
}

public class RaritySuccess
{
    public short FromRarity { get; set; }
    public int SuccessChance { get; set; }
}