using System.Collections.Generic;
using WingsAPI.Packets.Enums.Shells;

namespace NosEmu.Plugins.BasicImplementations.Algorithms.Shells;

public class ShellCategoryConfiguration : List<ShellCategoryInfo>
{
}

public class ShellCategoryInfo
{
    public ShellEffectCategory EffectCategory { get; set; }
    public List<ShellEffectType> PossibleEffects { get; set; }
}