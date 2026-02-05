using System.Collections.Generic;
using WingsEmu.Game._enum;

namespace WingsEmu.Game.Configurations.Act7.CarvedRunes;

public class WeaponRuneCardConfiguration
{
    public List<WeaponRuneCard> Cards { get; set; }
}

public class WeaponRuneCard
{
    public byte Type { get; set; }
    
    public byte SubType { get; set; }
    
    public List<int> FirstData { get; set; }
    
    public List<int> SecondData { get; set; }

    public BCardScalingType FirstDataScalingType { get; set; }
    
    public bool IsRunePower { get; set; }
    
    public byte CastType { get; set; }
}