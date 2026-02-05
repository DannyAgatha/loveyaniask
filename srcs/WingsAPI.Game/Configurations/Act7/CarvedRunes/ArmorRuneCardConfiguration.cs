using System.Collections.Generic;
using WingsEmu.Game._enum;
using WingsEmu.Game.Buffs;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Configurations.Act7.CarvedRunes;

public class ArmorRuneCardConfiguration
{
    public List<ArmorRuneCard> Cards { get; set; }
}

public class ArmorRuneCard
{
    public byte Type { get; set; }
    
    public byte SubType { get; set; }
    
    public List<int> FirstData { get; set; }
    
    public List<int> SecondData { get; set; }

    public BCardScalingType FirstDataScalingType { get; set; }
    
    public bool IsRunePower { get; set; }
    
    public byte CastType { get; set; }
}