using System.Collections.Generic;

namespace WingsEmu.Game.Families.Configuration;

public class FamilyLevelBuffConfiguration
{
    public List<FamilyLevelBuff> FamilyLevelBuffs { get; set; } = [];
}

public class FamilyLevelBuff
{
    public int Level { get; set; }
    public List<int> BuffVnums { get; set; } = [];
}