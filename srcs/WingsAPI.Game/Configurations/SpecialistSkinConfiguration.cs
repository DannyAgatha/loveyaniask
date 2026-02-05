using System.Collections.Generic;

namespace WingsEmu.Game.Configurations;

public class SpecialistSkinConfiguration
{
    public List<SpecialistSkin> SpecialistSkins { get; set; }
}

public class SpecialistSkin
{
    public int SpVnum { get; set; }
    public int SpSkinMorph { get; set; }
}