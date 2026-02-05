using System.Collections.Generic;

namespace WingsEmu.Game.Cellons;

public class CellonSystemConfiguration
{
    public HashSet<CellonPossibilities> Options { get; set; }
    public HashSet<CellonChances> ChancesToSuccess { get; set; }
}