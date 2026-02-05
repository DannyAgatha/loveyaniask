using System;

namespace WingsEmu.Game.Helpers;

public class MapLocation
{
    public Guid MapInstanceId { get; init; }
    public short X { get; init; }
    public short Y { get; init; }
}