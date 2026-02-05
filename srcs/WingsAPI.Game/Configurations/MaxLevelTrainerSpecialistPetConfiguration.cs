using System.Collections.Generic;

namespace WingsEmu.Game.Configurations;

public class PetMaxLevelConfiguration
{
    public IEnumerable<MaxPetLevelConfiguration> Configurations { get; set; }
}

public class MaxPetLevelConfiguration
{
    public byte Stars { get; set; }
    public int ItemVnum { get; set; }
    public int GoldRequired { get; set; }
    public short Quantity { get; set; }
    public byte MaxLevel { get; set; }
}