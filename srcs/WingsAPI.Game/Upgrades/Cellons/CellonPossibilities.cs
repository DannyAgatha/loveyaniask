using System.Collections.Generic;

namespace WingsEmu.Game.Cellons;

public class CellonPossibilities
{
    public int CellonLevel { get; set; }
    public int Price { get; set; }
    public HashSet<CellonOption> Options { get; set; }
}