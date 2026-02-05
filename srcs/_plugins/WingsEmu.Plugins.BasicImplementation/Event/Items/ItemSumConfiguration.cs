using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NosEmu.Plugins.BasicImplementations.Event.Items;

[DataContract]
public class ItemSumConfiguration : List<ItemSumMats>
{
}

public class ItemSumMats
{
    public int SumUpgrade { get; set; }
    public int Gold { get; set; }
    public int RiverSand { get; set; }
    public int SuccessChance { get; set; }
}