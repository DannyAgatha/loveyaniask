namespace WingsEmu.Game.Act4.Entities;

public class CalvinasDragon
{
    public CoordType Axis { get; set; }
    public short At { get; set; }
    public byte Size { get; set; }
    public short Start { get; set; }
    public short End { get; set; }
}

public enum CoordType
{
    X,
    Y
}