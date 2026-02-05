namespace WingsEmu.Game.Helpers.Damages;

public struct Position
{
    public Position(short x, short y)
    {
        X = x;
        Y = y;
    }

    public short X { get; }
    public short Y { get; }

    public static bool operator ==(Position a, Position b) => a.Equals(b);

    public static bool operator !=(Position a, Position b) => !a.Equals(b);

    public bool Equals(Position other) => X == other.X && Y == other.Y;

    public override bool Equals(object other) => Equals((Position)other);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + X.GetHashCode();
            hash = hash * 23 + Y.GetHashCode();
            return hash;
        }
    }
}