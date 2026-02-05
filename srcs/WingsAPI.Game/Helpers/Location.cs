namespace WingsEmu.Game.Helpers;

public struct Location
{
    public Location(int mapId, short x, short y)
    {
        MapId = mapId;
        X = x;
        Y = y;
    }

    public int MapId { get; }
    public short X { get; }
    public short Y { get; }
}