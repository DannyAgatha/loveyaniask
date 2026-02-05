using System.Collections.Generic;

namespace WingsEmu.Game.Configurations.Act7.Tattoos;
public class TattooOptionsConfiguration
{
    public List<ListTattooOptions> Tattoos { get; set; }
}

public class ListTattooOptions
{
    public int ItemVnum { get; set; }
    public byte CastId { get; set; }
    public List<int> TattooOptions { get; set; }
}