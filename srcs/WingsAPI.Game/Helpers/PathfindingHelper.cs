using System;
using System.Collections.Generic;
using WingsEmu.Game.Helpers.Damages;

namespace WingsEmu.Game.Helpers;

public static class PathfindingAlgorithm
{
    [Flags]
    public enum MapCellFlags
    {
        IsWalkingDisabled = 0x1,
        IsAttackDisabledThrough = 0x2,
        UnknownYet = 0x4,
        IsMonsterAggroDisabled = 0x8,
        IsPvpDisabled = 0x10,
        MateDoll = 0x20
    }

    private static readonly sbyte[,] Neighbours =
    {
        { -1, -1 }, { 0, -1 }, { 1, -1 },
        { -1, 0 }, { 1, 0 },
        { -1, 1 }, { 0, 1 }, { 1, 1 }
    };

    public static bool IsWalkable(this IReadOnlyList<byte> array, int x, int y, int width, int height)
    {
        if (x < 0 || y < 0)
        {
            return false;
        }

        if (x >= width || y >= height)
        {
            return false;
        }

        return (array[x + y * width] & (int)MapCellFlags.IsWalkingDisabled) == 0;
    }

    public static bool IsWalkable(this byte cell) => (cell & (int)MapCellFlags.IsWalkingDisabled) == 0;

    public static bool IsPvpZoneOff(this IReadOnlyList<byte> array, int x, int y, int width, int height)
    {
        if (x < 0 || y < 0)
        {
            return true;
        }

        if (x >= width || y >= height)
        {
            return true;
        }

        return (array[x + y * width] & (int)MapCellFlags.IsPvpDisabled) == (int)MapCellFlags.IsPvpDisabled;
    }

    public static bool IsMateDollZone(this IReadOnlyList<byte> array, int x, int y, int width, int height)
    {
        if (x < 0 || y < 0)
        {
            return false;
        }

        if (x >= width || y >= height)
        {
            return false;
        }

        return (array[x + y * width] & (int)MapCellFlags.MateDoll) == (int)MapCellFlags.MateDoll;
    }

    public static bool IsMonsterAggroDisabled(this IReadOnlyList<byte> array, int x, int y, int width, int height)
    {
        if (x < 0 || y < 0)
        {
            return true;
        }

        if (x >= width || y >= height)
        {
            return true;
        }

        return (array[x + y * width] & (int)MapCellFlags.IsMonsterAggroDisabled) == 0;
    }

    /// <summary>
    ///     Returns an Array with the neighbors of the given position
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="map"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static IReadOnlyList<Position> GetNeighbors(this Position pos, IReadOnlyList<byte> map, int width, int height)
    {
        var neighbors = new List<Position>(8);
        for (byte i = 0; i < 8; i++)
        {
            short x = (short)(pos.X + Neighbours[i, 0]);
            short y = (short)(pos.Y + Neighbours[i, 1]);
            if (x >= 0 && x < width && y >= 0 && y < height && map.IsWalkable(x, y, width, height))
            {
                neighbors.Add(new Position(x, y));
            }
        }

        return neighbors;
    }

    public static Position Bresenham(Position from, Position to, float maxCalculationDistance, IReadOnlyList<byte> grid, int width, int height, bool ret)
    {
        short sX = from.X;
        short sY = from.Y;
        short tX = to.X;
        short tY = to.Y;

        int dx = Math.Abs(tX - sX);
        int sx = sX < tX ? 1 : -1;
        int dy = -Math.Abs(tY - sY);
        int sy = sY < tY ? 1 : -1;
        int err = dx + dy;

        short lastX = -1;
        short lastY = -1;

        short lastFreeX = from.X;
        short lastFreeY = from.Y;

        while (true) // Bresenham
        {
            double distance = from.GetDoubleDistance(new Position(sX, sY));
            if (lastX != -1 && lastY != -1 && distance >= maxCalculationDistance)
            {
                return new Position(lastFreeX, lastFreeY);
            }

            if (ret)
            {
                if (grid.IsWalkable(sX, sY, width, height))
                {
                    lastFreeX = sX;
                    lastFreeY = sY;
                }
            }
            else
            {
                if (!grid.IsWalkable(sX, sY, width, height))
                {
                    return new Position(lastX, lastY);
                }
            }

            lastX = sX;
            lastY = sY;

            if (sX == tX && sY == tY)
            {
                break;
            }

            int e2 = 2 * err;
            if (e2 >= dy)
            {
                err += dy;
                sX += (short)sx;
            }

            if (e2 > dx)
            {
                continue;
            }

            err += dx;
            sY += (short)sy;
        }

        return new Position(lastFreeX, lastFreeY);
    }
}