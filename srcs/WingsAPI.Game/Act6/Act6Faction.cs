using System;

namespace WingsEmu.Game.Act6;

public class Act6Faction
{
    public double Points { get; set; }
    public byte Mode { get; set; }
    public short TotalTime { get; set; }
    public int CurrentTime { get; set; }
    public DateTime TimeOpen { get; set; }
}