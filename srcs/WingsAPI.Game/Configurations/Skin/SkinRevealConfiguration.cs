using System;
using System.Collections.Generic;
using System.Linq;
using WingsEmu.Packets.Enums.Character;

namespace WingsEmu.Game.Configurations.Skin;

public class SkinRevealConfiguration
{
    public List<SkinRevealItem> SkinReveal { get; set; } = [];

    public int? GetItemVnumForClass(int itemVnum, ClassType classType)
    {
        SkinRevealItem revealItem = SkinReveal.FirstOrDefault(item => item.ChestVnum == itemVnum);
        if (revealItem == null)
        {
            return null;
        }

        string classKey = Enum.GetName(typeof(ClassType), classType)?
            .Select((ch, i) => i > 0 && char.IsUpper(ch) ? $"_{char.ToLower(ch)}" : char.ToLower(ch).ToString())
            .Aggregate((a, b) => a + b);

        return classKey != null ? revealItem.ClassItems.GetValueOrDefault(classKey) : null;
    }
}

public class SkinRevealItem
{
    public int ChestVnum { get; set; }
    public Dictionary<string, int?> ClassItems { get; set; } = new();
}